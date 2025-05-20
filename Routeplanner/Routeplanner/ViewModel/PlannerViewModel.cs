using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Model;
using Routeplanner.Services;
using Routeplanner.Services.Database;
using Routeplanner.Services.Planner;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Routeplanner.ViewModel
{
    public partial class PlannerViewModel : ObservableObject
    {
        private readonly ITripService _tripService;

        private readonly StationTable _stationTable;

        private readonly SavedTripsTable _savedTripsTable;

        private readonly RouteCacheTable _routeCacheTable;

        public ObservableCollection<Trip> Trips { get; } = new ObservableCollection<Trip>();

        public List<Route> _RouteCache { get; } = new();

        private List<string> _stationCache = new();

        [ObservableProperty]
        private string _origin;

        [ObservableProperty]
        private string _destination;

        [ObservableProperty]
        private TimeSpan _selectedTime;

        [ObservableProperty]
        private DateTime _selectedDate;

        [ObservableProperty]
        private DateTime _MinDate;

        [ObservableProperty]
        private DateTime _MaxDate;

        [ObservableProperty]
        private string _selectedType;

        [ObservableProperty]
        private ObservableCollection<string> _originSuggestions = new();

        [ObservableProperty]
        private ObservableCollection<string> _destinationSuggestions = new();

        [ObservableProperty]
        private bool _isOriginSuggestionsVisible;

        [ObservableProperty]
        private bool _isDestinationSuggestionsVisible;

        [ObservableProperty]
        private bool _isRouteCacheVisible;

        [ObservableProperty]
        private string saveButtonText = "Save Trip";

        [ObservableProperty]
        public bool _loading;

        public PlannerViewModel(ITripService tripService, StationTable stationTable, RouteCacheTable routeCacheTable, SavedTripsTable savedTripsTable)
        {
            _tripService = tripService;
            _stationTable = stationTable;
            _routeCacheTable = routeCacheTable;
            _savedTripsTable = savedTripsTable;

            // Set default date range
            _MinDate = DateTime.Today;
            _MaxDate = DateTime.Today.AddYears(1);
            SelectedDate = DateTime.Today;
            SelectedTime = DateTime.Now.TimeOfDay;
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            await Task.Run(CacheStationsAsync);
            await Task.Run(GetRoutesFromCacheAsync);
        }

        private async Task CacheStationsAsync()
        {
            var stations = await _stationTable.GetAllStations();
            _stationCache = stations.Select(s => s.Name)
            .Distinct()
            .ToList();
        }

        private async Task GetRoutesFromCacheAsync()
        {
            var routes = await _routeCacheTable.GetLast5Routes();
            foreach (var route in routes)
            {
                _RouteCache.Add(route);
            }
        }

        // Handlers for text changes
        partial void OnOriginChanged(string value)
        {
            UpdateSuggestions(value, true);
            IsRouteCacheVisible = true;
        }

        partial void OnDestinationChanged(string value)
        {
            UpdateSuggestions(value, false);
            IsRouteCacheVisible = true;
        }

        [RelayCommand]
        private void Completed()
        {
            HideAllSuggestions();
            IsRouteCacheVisible = false;
        }

        [RelayCommand]
        private void SelectOrigin(string selectedItem)
        {
            Origin = selectedItem;
            IsOriginSuggestionsVisible = false;
            IsRouteCacheVisible = false;
        }

        [RelayCommand]
        private void SelectDestination(string selectedItem)
        {
            Destination = selectedItem;
            IsDestinationSuggestionsVisible = false;
            IsRouteCacheVisible = false;
        }

        [RelayCommand]
        private void SelectRoute(Route selectedItem)
        {
            Origin = selectedItem.FromStation;
            Destination = selectedItem.ToStation;

            IsOriginSuggestionsVisible = false;
            IsDestinationSuggestionsVisible = false;
        }

        [RelayCommand]
        private async Task GoToTripAsync(Trip selectedItem)
        {
            if (selectedItem == null) return;

            var viewModel = new TripDetailsViewModel(selectedItem);
            var page = new TripDetailsPage(viewModel)
            {
                BindingContext = viewModel
            };

            await Application.Current.MainPage.Navigation.PushAsync(page);
        }

        [RelayCommand]
        private async Task Search()
        {
            // remove route cache from screen
            IsRouteCacheVisible = false;
            if (string.IsNullOrWhiteSpace(Origin) || string.IsNullOrWhiteSpace(Destination))
            {
                Console.WriteLine("Please enter valid station names.");
                return;
            }

            try
            {
                Loading = true;
                await Task.Run(async () => {
                    // Save route to cache
                    Route route = new Route
                    {
                        FromStation = Origin,
                        ToStation = Destination
                    };

                    await _routeCacheTable.SaveRouteToCacheAsync(route);

                    // Update UI collection on main thread
                    MainThread.BeginInvokeOnMainThread(() => {
                        _RouteCache.Add(route);
                    });

                    // Search query
                    //string startCode = await _stationTable.NameToCode(Origin);
                    string startCode = await _stationTable.NameToCode(Origin);
                    string destinationCode = await _stationTable.NameToCode(Destination);
                    Console.WriteLine(startCode, destinationCode);

                    var parameters = new APIParameters
                    {
                        FromStation = startCode,
                        ToStation = destinationCode,
                        SelectedDate = SelectedDate,
                        SelectedTime = SelectedTime
                    };

                    string response = await _tripService.FetchTripsAsync(parameters);
                    JsonDocument apiResponse = JsonDocument.Parse(response);

                    List<Trip> trips = _tripService.ExtractTripsFromApiResponse(apiResponse);
                    Console.WriteLine(trips.Count());
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (Trips.Count != 0)
                            Trips.Clear();

                        foreach (var trip in trips)
                        {
                            Trips.Add(trip);
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

        private void UpdateSuggestions(string query, bool isorigin)
        {
            // use station cache to update suggestions

            var results = _stationCache
               .Where(s => s.StartsWith(query, StringComparison.OrdinalIgnoreCase))
               .Take(10) // limit the number of suggestions
               .Distinct()
               .ToList();
            if (isorigin)
            {
                OriginSuggestions.Clear();
                foreach (var item in results)
                    OriginSuggestions.Add(item);

                // toon suggesties als er resultaten zijn en de query niet leeg is
                IsOriginSuggestionsVisible = results.Any() && !string.IsNullOrEmpty(query);
            }
            else
            {
                DestinationSuggestions.Clear();
                foreach (var item in results)
                    DestinationSuggestions.Add(item);

                // toon suggesties als er resultaten zijn en de query niet leeg is
                IsDestinationSuggestionsVisible = results.Any() && !string.IsNullOrEmpty(query);
            }
        }

        [RelayCommand]
        private async Task SaveAsync(Trip trip)
        {
            if (trip.SaveButtonText == "Save Trip")
            {
                trip.SaveButtonText = "Unsave Trip";
                // Save the trip to the database
                await _savedTripsTable.SaveTripAsync(trip);
            }
            else
            {
                trip.SaveButtonText = "Save Trip";
                await _savedTripsTable.RemoveTripAsync(trip);
            }
        }

        [RelayCommand]
        private async Task Switch()
        {
            string start = Origin;
            string end = Destination;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Origin = end;
                Destination = start;
            });
            HideAllSuggestions();
        }
        private void HideAllSuggestions()
        {
            IsOriginSuggestionsVisible = false;
            IsDestinationSuggestionsVisible = false;
        }
    }
}