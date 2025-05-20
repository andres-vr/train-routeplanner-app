using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Model;
using Routeplanner.Services.Departures;
using Routeplanner.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using Routeplanner.Services.Database;

namespace Routeplanner.ViewModel
{
    public partial class DeparturesViewModel : ObservableObject
    {
        private readonly IDepartureService _departureService;

        private readonly StationTable _stationTable;

        private readonly SavedDeparturesTable _savedDeparturesTable;

        public ObservableCollection<Departure> _Departures { get; } = new();

        private List<string> _stationCache = new();

        public Departure Model { get; }

        [ObservableProperty]
        private string _station;

        [ObservableProperty]
        private ObservableCollection<string> _stationSuggestions = new();

        [ObservableProperty]
        private bool _isStationSuggestionsVisible;

        [ObservableProperty]
        public bool _loading;

        public DeparturesViewModel(IDepartureService departureService, StationTable stationTable, SavedDeparturesTable savedDepartureTable)
        {
            _departureService = departureService;
            _stationTable = stationTable;
            _savedDeparturesTable = savedDepartureTable;
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            await Task.Run(CacheStationsAsync);
        }

        private async Task CacheStationsAsync()
        {
            var stations = await _stationTable.GetAllStations();
            _stationCache = stations.Select(s => s.Name).ToList();
        }

        //Handler for text changes
        partial void OnStationChanged(string value)
        {
            UpdateSuggestions(value, true);
        }
        [RelayCommand]
        private void Completed() => HideAllSuggestions();

        [RelayCommand]
        private void SelectStation(string selectedItem)
        {
            Station = selectedItem;
            HideAllSuggestions();
        }

        [RelayCommand]
        private async Task GoToDepartureAsync(Departure selectedItem)
        {
            if (selectedItem == null) return;

            var viewModel = new DepartureDetailsViewModel(selectedItem);
            var page = new DepartureDetailsPage(viewModel)
            {
                BindingContext = viewModel
            };

            await Application.Current.MainPage.Navigation.PushAsync(page);
        }
        [RelayCommand]
        private async Task SaveAsync(Departure departure)
        {
            if (departure.SaveButtonText == "Save Departure")
            {
                departure.SaveButtonText = "Unsave Departure";
                await _savedDeparturesTable.SaveDepartureAsync(departure);
            }
            else
            {
                departure.SaveButtonText = "Save Departure";
                await _savedDeparturesTable.RemoveDepartureAsync(departure);
            }
        }
        [RelayCommand]
        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(_station))
            {
                Console.WriteLine("Please enter valid station names.");
                return;
            }

            try
            {
                Loading = true;
                await Task.Run(async () => {
                    string station = await _stationTable.NameToCode(_station);

                    APIParameters parameters = new APIParameters
                    {
                        FromStation = station
                    };
                    string response = await _departureService.FetchDeparturesAsync(parameters);

                    Console.Write(response);
                    JsonDocument apiResponse = JsonDocument.Parse(response);
                    List<Departure> departures = _departureService.ExtractDeparturesFromApiResponse(apiResponse, _station);
                    MainThread.BeginInvokeOnMainThread(() => {
                        if (_Departures.Count != 0)
                            _Departures.Clear();
                        foreach (var departure in departures)
                        {
                            _Departures.Add(departure);
                        }
                        Loading = false;
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        private void UpdateSuggestions(string query, bool isStation)
        {
            // Handle empty query
            if (string.IsNullOrEmpty(query))
            {
                if (isStation)
                {
                    StationSuggestions.Clear();
                    IsStationSuggestionsVisible = false;
                }
                return;
            }

            // use station cache to update suggestions
            var results = _stationCache
               .Where(s => s.StartsWith(query, StringComparison.OrdinalIgnoreCase))
               .Take(10) // limit the number of suggestion
               .Distinct()
               .ToList();

            if (isStation)
            {
                StationSuggestions.Clear();
                foreach (var item in results)
                    StationSuggestions.Add(item);

                // toon suggesties als er resultaten zijn 
                IsStationSuggestionsVisible = true;
            }
        }

        private void HideAllSuggestions()
        {
            IsStationSuggestionsVisible = false;
        }
    }
}