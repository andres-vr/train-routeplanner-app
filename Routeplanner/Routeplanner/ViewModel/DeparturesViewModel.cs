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
        private string saveIconGlyph = "\ue158";

        public DeparturesViewModel(IDepartureService departureService, StationTable stationTable, SavedDeparturesTable savedDepartureTable)
        {
            _departureService = departureService;
            _stationTable = stationTable;
            _savedDeparturesTable = savedDepartureTable;

            Task.Run(CacheStationsAsync);
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
            Console.WriteLine("hoi");
            if (string.IsNullOrWhiteSpace(_station))
            {
                Console.WriteLine("Please enter valid station names.");
                return;
            }

            try
            {
                await Task.Run(async () => {
                    string station = await _stationTable.NameToCode(_station);

                    APIParameters parameters = new APIParameters
                    {
                        FromStation = station
                    };
                    string response = await _departureService.FetchDeparturesAsync(parameters);

                    Console.Write(response);
                    JsonDocument apiResponse = JsonDocument.Parse(response);
                    List<Departure> departures = ExtractDeparturesFromApiResponse(apiResponse, _station);
                    MainThread.BeginInvokeOnMainThread(() => {
                        if (_Departures.Count != 0)
                            _Departures.Clear();
                        foreach (var departure in departures)
                        {
                            _Departures.Add(departure);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public static List<Departure> ExtractDeparturesFromApiResponse(JsonDocument responseData, string currentStation)
        {
            List<Departure> departuresList = new List<Departure>();

            try
            {
                // Add logging to track method calls
                Console.WriteLine("Starting ExtractDeparturesFromApiResponse");

                // Get the departures array
                JsonElement departuresArray = responseData.RootElement.GetProperty("payload").GetProperty("departures");

                // Track for debugging
                Console.WriteLine($"Processing {departuresArray.GetArrayLength()} departures");

                // Iterate through all departures in the response
                for (int i = 0; i < departuresArray.GetArrayLength(); i++)
                {
                    Console.WriteLine($"Processing departure {i + 1}");
                    var departureData = departuresArray[i];

                    DateTime time = DateTime.Parse(departureData.GetProperty("actualDateTime").GetString());
                    string formattedTime = time.ToString("HH:mm");

                    try
                    {
                        // Create a new Departure object for each departure in the API response
                        Departure departure = new Departure
                        {
                            Time = TimeSpan.Parse(formattedTime),
                            Origin = currentStation,
                            Destination = departureData.GetProperty("direction").GetString(),
                            TrainType = departureData.GetProperty("product").GetProperty("longCategoryName").GetString(),
                            Track = departureData.GetProperty("actualTrack").GetString(),
                            Stops = new List<DepartureStop>()
                        };

                        // Process all route stations for this departure
                        ProcessRouteStationsForDeparture(departureData, departure, currentStation);

                        // Add the complete departure to our list
                        departuresList.Add(departure);
                        Console.WriteLine($"Departure {i + 1} processed with {departure.Stops.Count} stops");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing departure {i + 1}: {ex.Message}");
                        // Continue with the next departure
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExtractDeparturesFromApiResponse: {ex.Message}");
            }

            return departuresList;
        }

        private static void ProcessRouteStationsForDeparture(JsonElement departureData, Departure departure, string currentStation)
        {
            // start station
            departure.Stops.Add(new DepartureStop { StopName = currentStation });
            Console.WriteLine($"Added origin stop: {currentStation}");

            // Check if route stations are available
            if (departureData.TryGetProperty("routeStations", out JsonElement routeStations) &&
                routeStations.GetArrayLength() > 0)
            {
                Console.WriteLine($"Processing {routeStations.GetArrayLength()} route stations for departure");

                // Process intermediate stops
                for (int k = 0; k < routeStations.GetArrayLength(); k++)
                {
                    var station = routeStations[k];
                    string stationName = station.GetProperty("mediumName").GetString();

                    departure.Stops.Add(new DepartureStop { StopName = stationName });
                    Console.WriteLine($"Added intermediate stop: {stationName}");

                }
            }
            else
            {
                Console.WriteLine("No route stations found for this departure");
            }

            // Add destination 
            string destinationName = departureData.GetProperty("direction").GetString();
            departure.Stops.Add(new DepartureStop { StopName = destinationName });
            Console.WriteLine($"Added destination stop: {destinationName}");
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
               .Take(10) // limit the number of suggestions
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