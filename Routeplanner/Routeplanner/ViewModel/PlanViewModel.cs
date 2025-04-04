using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Model;
using Routeplanner.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Routeplanner.ViewModel
{
    public partial class PlanViewModel : ObservableObject
    {
        private readonly ITripService _tripService;

        private readonly SqliteDatabaseService _databaseService;

        public ObservableCollection<Trip> _Trips { get; } = new();

        private List<string> _stationCache = new();

        [ObservableProperty]
        private string _startPoint;

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
        private ObservableCollection<string> _startPointSuggestions = new();

        [ObservableProperty]
        private ObservableCollection<string> _destinationSuggestions = new();

        [ObservableProperty]
        private bool _isStartPointSuggestionsVisible;

        [ObservableProperty]
        private bool _isDestinationSuggestionsVisible;

        public PlanViewModel(ITripService tripService, SqliteDatabaseService databaseService)
        {
            _tripService = tripService;
            _databaseService = databaseService;

            // Set default date range
            _MinDate = DateTime.Today;
            _MaxDate = DateTime.Today.AddYears(1);
            SelectedDate = DateTime.Today;
            SelectedTime = DateTime.Now.TimeOfDay;

            Task.Run(CacheStationsAsync);
        }

        private async Task CacheStationsAsync()
        {
            var stations = await _databaseService.GetAllStations();
            _stationCache = stations.Select(s => s.name).ToList();
        }

        // Handlers for text changes
        partial void OnStartPointChanged(string value) =>
            UpdateSuggestions(value, true);

        partial void OnDestinationChanged(string value) =>
            UpdateSuggestions(value, false);

        [RelayCommand]
        private void Completed() => HideAllSuggestions();

        [RelayCommand]
        private void SelectStartPoint(string selectedItem)
        {
            StartPoint = selectedItem;
            IsStartPointSuggestionsVisible = false;
        }

        [RelayCommand]
        private void SelectDestination(string selectedItem)
        {
            Destination = selectedItem;
            IsDestinationSuggestionsVisible = false;
        }

        [RelayCommand]
        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(StartPoint) || string.IsNullOrWhiteSpace(Destination))
            {
                Console.WriteLine("Please enter valid station names.");
                return;
            }

            try
            {
                Console.WriteLine("hoi");
                string startCode = await _databaseService.NameToCode(StartPoint);
                string destinationCode = await _databaseService.NameToCode(Destination);
                Console.WriteLine(startCode, destinationCode);
                var parameters = new APIParameters
                {
                    fromStation = startCode,
                    toStation = destinationCode,
                    selectedDate = SelectedDate,
                    selectedTime = SelectedTime
                };

                string response = await _tripService.FetchTripsAsync(parameters);

                JsonDocument apiResponse = JsonDocument.Parse(response);

                List<Trip> trips = ExtractTripsFromApiResponse(apiResponse);
                if (_Trips.Count != 0)
                    _Trips.Clear();
                foreach (var trip in trips)
                {
                    _Trips.Add(trip);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public static List<Trip> ExtractTripsFromApiResponse(JsonDocument responseData)
        {
            List<Trip> tripsList = new List<Trip>();

            try
            {
                // Add logging to track method calls
                Console.WriteLine("Starting ExtractTripsFromApiResponse");

                // Get the trips array
                JsonElement tripsArray = responseData.RootElement.GetProperty("trips");

                // Track for debugging
                Console.WriteLine($"Processing {tripsArray.GetArrayLength()} trips");

                // Iterate through all trips in the response
                for (int i = 0; i < tripsArray.GetArrayLength(); i++)
                {
                    Console.WriteLine($"Processing trip {i + 1}");
                    var tripData = tripsArray[i];

                    // Create a new Trip object for each trip in the API response
                    Trip trip = new Trip
                    {
                        // Basic properties as before
                        startStation = tripData.GetProperty("legs")[0].GetProperty("origin").GetProperty("name").GetString(),
                        endStation = tripData.GetProperty("legs")[tripData.GetProperty("legs").GetArrayLength() - 1]
                                           .GetProperty("destination").GetProperty("name").GetString(),
                        startTime = DateTime.Parse(tripData.GetProperty("legs")[0]
                                           .GetProperty("origin").GetProperty("actualDateTime").GetString())
                                           .ToString("yyyy-MM-dd HH:mm:ss"),
                        endTime = DateTime.Parse(tripData.GetProperty("legs")[tripData.GetProperty("legs").GetArrayLength() - 1]
                                           .GetProperty("destination").GetProperty("actualDateTime").GetString())
                                           .ToString("yyyy-MM-dd HH:mm:ss"),
                        duration = $"{tripData.GetProperty("actualDurationInMinutes").GetInt32()} minutes",
                        connections = tripData.GetProperty("transfers").GetInt32(),
                        // Initialize stopList here
                        stopList = new Dictionary<string, DateTime>()
                    };

                    // Process all stops for this trip at once
                    ProcessAllStopsForTrip(tripData, trip);

                    // Add the complete trip to our list
                    tripsList.Add(trip);
                    Console.WriteLine($"Trip {i + 1} processed with {trip.stopList.Count} stops");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExtractTripsFromApiResponse: {ex.Message}");
            }

            return tripsList;
        }

        private static void ProcessAllStopsForTrip(JsonElement tripData, Trip trip)
        {
            // Create the HashSet for this specific trip
            HashSet<string> processedStops = new HashSet<string>();

            int legCount = tripData.GetProperty("legs").GetArrayLength();
            Console.WriteLine($"Processing {legCount} legs for trip");

            // Process all legs
            for (int j = 0; j < legCount; j++)
            {
                var leg = tripData.GetProperty("legs")[j];
                var stops = leg.GetProperty("stops");

                // Process stops for this leg
                for (int k = 0; k < stops.GetArrayLength(); k++)
                {
                    var stop = stops[k];
                    string stationName = stop.GetProperty("name").GetString();

                    // Get time logic
                    DateTime stopTime;
                    if (stop.TryGetProperty("actualArrivalDateTime", out JsonElement arrivalTimeElement))
                    {
                        stopTime = DateTime.Parse(arrivalTimeElement.GetString());
                    }
                    else if (stop.TryGetProperty("actualDepartureDateTime", out JsonElement departureTimeElement))
                    {
                        stopTime = DateTime.Parse(departureTimeElement.GetString());
                    }
                    else
                    {
                        continue;
                    }

                    // Check if we've already processed this station
                    if (!processedStops.Contains(stationName))
                    {
                        trip.stopList[stationName] = stopTime;
                        processedStops.Add(stationName);
                        Console.WriteLine($"Added stop: {stationName}");
                    }
                    else
                    {
                        Console.WriteLine($"Skipped duplicate stop: {stationName}");
                    }
                }
            }
        }

        private async void UpdateSuggestions(string query, bool isStartPoint)
        {
            // use station cache to update suggestions

            var results = _stationCache
               .Where(s => s.StartsWith(query, StringComparison.OrdinalIgnoreCase)) 
               .Take(10) // limit the number of suggestions
               .ToList();
            if (isStartPoint)
            {
                StartPointSuggestions.Clear();
                foreach (var item in results)
                    StartPointSuggestions.Add(item);

                // toon suggesties als er resultaten zijn en de query niet leeg is
                IsStartPointSuggestionsVisible = results.Any() && !string.IsNullOrEmpty(query);
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

        private void HideAllSuggestions()
        {
            IsStartPointSuggestionsVisible = false;
            IsDestinationSuggestionsVisible = false;
        }
    }
}