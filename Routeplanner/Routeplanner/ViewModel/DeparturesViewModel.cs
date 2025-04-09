using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Model;
using Routeplanner.Services.Database;
using Routeplanner.Services.Departures;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Routeplanner.ViewModel
{
    public partial class DeparturesViewModel : ObservableObject
    {
        private readonly IDepartureService _departureService;

        private readonly SqliteDatabaseService _databaseService;

        public ObservableCollection<Departure> _Departures { get; } = new();

        private List<string> _stationCache = new();

        [ObservableProperty]
        private string _station;

        [ObservableProperty]
        private ObservableCollection<string> _stationSuggestions = new();

        [ObservableProperty]
        private bool _isStationSuggestionsVisible;

        public DeparturesViewModel(IDepartureService departureService, SqliteDatabaseService databaseService)
        {
            _departureService = departureService;
            _databaseService = databaseService;

            Task.Run(CacheStationsAsync);
        }

        private async Task CacheStationsAsync()
        {
            var stations = await _databaseService.GetAllStations();
            _stationCache = stations.Select(s => s.name).ToList();
        }

        // Handlers for text changes
        partial void OnStationChanged(string value) =>
            UpdateSuggestions(value, true);

        [RelayCommand]
        private void Completed() => HideAllSuggestions();

        [RelayCommand]
        private void SelectStation(string selectedItem)
        {
            _station = selectedItem;
            IsStationSuggestionsVisible = false;
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
                string station = await _databaseService.NameToCode(_station);

                APIParameters parameters = new APIParameters
                {
                    fromStation = station
                };
                string response = await _departureService.FetchDeparturesAsync(parameters);

                Console.Write(response);
                JsonDocument apiResponse = JsonDocument.Parse(response);
                List<Departure> departures = ExtractDeparturesFromApiResponse(apiResponse, _station);
                if (_Departures.Count != 0)
                    _Departures.Clear();
                foreach (var departure in departures)
                {
                    _Departures.Add(departure);
                }
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

                    try
                    {
                        // Create a new Departure object for each departure in the API response
                        Departure departure = new Departure
                        {
                            Time = DateTime.Parse(departureData.GetProperty("actualDateTime").GetString())
                            .ToString("HH:mm"),
                            Origin = currentStation,
                            Destination = departureData.GetProperty("direction").GetString(),
                            TrainType = departureData.GetProperty("product").GetProperty("longCategoryName").GetString(),
                            Track = departureData.GetProperty("actualTrack").GetString(),

                            // Initialize Stops list
                            Stops = new List<string>()
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
            // Create a HashSet to track processed stops and avoid duplicates
            HashSet<string> processedStops = new HashSet<string>();

            // First, add the origin station (current station where the API is called from)
            departure.Stops.Add(currentStation);
            processedStops.Add(currentStation);
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

                    // Check if we've already processed this station
                    if (!processedStops.Contains(stationName))
                    {
                        departure.Stops.Add(stationName);
                        processedStops.Add(stationName);
                        Console.WriteLine($"Added intermediate stop: {stationName}");
                    }
                    else
                    {
                        Console.WriteLine($"Skipped duplicate stop: {stationName}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No route stations found for this departure");
            }

            // Add destination as the final stop if not already in the list
            string destinationName = departureData.GetProperty("direction").GetString();
            if (!processedStops.Contains(destinationName))
            {
                departure.Stops.Add(destinationName);
                processedStops.Add(destinationName);
                Console.WriteLine($"Added destination stop: {destinationName}");
            }
        }

        private async void UpdateSuggestions(string query, bool isStation)
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

                // toon suggesties als er resultaten zijn en de query niet leeg is
                IsStationSuggestionsVisible = results.Any() && !string.IsNullOrEmpty(query);
            }
        }

        private void HideAllSuggestions()
        {
            IsStationSuggestionsVisible = false;
        }
    }
}