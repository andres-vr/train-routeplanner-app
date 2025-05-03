using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Model;
using Routeplanner.Services.Database;
using Routeplanner.Services.Planner;
using Routeplanner.Services;
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

            public List<Trip> _Trips { get; } = new();

            public List<Route> _RouteCache { get; } = new();

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

            [ObservableProperty]
            private bool _isRouteCacheVisible;

            [ObservableProperty]
            private string saveButtonText = "Save Trip";

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
                _stationCache = stations.Select(s => s.Name).ToList();
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
            partial void OnStartPointChanged(string value)
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
            private void SelectStartPoint(string selectedItem)
            {
                StartPoint = selectedItem;
                IsStartPointSuggestionsVisible = false;
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
            StartPoint = selectedItem.FromStation;
            Destination = selectedItem.ToStation;

            IsStartPointSuggestionsVisible = false;
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
                if (string.IsNullOrWhiteSpace(StartPoint) || string.IsNullOrWhiteSpace(Destination))
                    {
                        Console.WriteLine("Please enter valid station names.");
                        return;
                    }

                    try
                    {
                        await Task.Run(async () => {
                            // Save route to cache
                            Route route = new Route
                            {
                                FromStation = StartPoint,
                                ToStation = Destination
                            };

                            await _routeCacheTable.SaveRouteToCacheAsync(route);

                            // Update UI collection on main thread
                            MainThread.BeginInvokeOnMainThread(() => {
                                _RouteCache.Add(route);
                            });

                            // Search query
                            string startCode = await _stationTable.NameToCode(StartPoint);
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

                            List<Trip> trips = ExtractTripsFromApiResponse(apiResponse);

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                 if (_Trips.Count != 0)
                                 _Trips.Clear();
                                foreach (var trip in trips)
                                {
                                   _Trips.Add(trip);
                                }
                            });
                        });
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
                    if (responseData == null)
                    {
                        Console.WriteLine("trips not found");
                        return tripsList;
                    }
                    // Add logging to track method calls
                    Console.WriteLine("Starting ExtractTripsFromApiResponse");

                    // Get the trips array
                    JsonElement tripsArray = responseData.RootElement.GetProperty("trips");

                    // Track for debugging
                    Console.WriteLine($"Processing {tripsArray.GetArrayLength()} trips");

                    // Iterate through all trips in the response
                    for (int i = 0; i < tripsArray.GetArrayLength(); i++)
                    {
                        var tripData = tripsArray[i];
                    Console.WriteLine("1");

                    // Create a new Trip object for each trip in the API response
                    string startStation = tripData.GetProperty("legs")[0].GetProperty("origin").GetProperty("name").GetString();
                    Console.WriteLine("2");
                    string endStation = tripData.GetProperty("legs")[tripData.GetProperty("legs").GetArrayLength() - 1]
                                               .GetProperty("destination").GetProperty("name").GetString();
                    /*Console.WriteLine("3");
                    string dateTimeStr = tripData.GetProperty("legs")[0]
                               .GetProperty("origin")
                               .GetProperty("actualDateTime")
                               .GetString();

                    DateTime startTime = DateTime.Parse(dateTimeStr);

                    string formattedTime = startTime.ToString("HH:mm");

                    Console.WriteLine("4");
                    string dateTimeStr = (tripData.GetProperty("legs")[tripData.GetProperty("legs").GetArrayLength() - 1]
                                           .GetProperty("destination").GetProperty("actualDateTime").GetString());
                    DateTime endTime = DateTime.Parse(dateTimeStr);

                    string formattedEndTime = endTime.ToString("HH:mm");
                    */
                    string formattedStartTime = "12:00";
                    string formattedEndTime = "12:00";
                    string track;
                    if (tripData.TryGetProperty("actualTrack", out JsonElement actualTrack))
                    {
                        track = actualTrack.ToString();
                    }
                    else if (tripData.TryGetProperty("plannedTrack", out JsonElement plannedTrack))
                    {
                        track = plannedTrack.ToString();    
                    }
                    else
                    {
                        track = "Unknown track";
                    }
                    Console.Write("no track issues");
                    TimeSpan duration;

                    if (tripData.TryGetProperty("actualDurationInMinutes", out JsonElement durationElement))
                    {
                        duration = TimeSpan.FromMinutes(durationElement.GetInt32());
                    }
                    else if (tripData.TryGetProperty("plannedDurationInMinutes", out JsonElement plannedDurationElement))
                    {
                        duration = TimeSpan.FromMinutes(plannedDurationElement.GetInt32());
                    }
                    else
                    {
                        duration = TimeSpan.Zero; 
                    }
                    Console.WriteLine("5");
                    int connections = 0;
                    if (tripData.TryGetProperty("transfers", out JsonElement transfersElement))
                    {
                        connections = transfersElement.GetInt32();
                    }
                    Console.WriteLine("6");
                    Trip trip = new Trip
                    {
                        // Basic properties as before
                        StartStation = startStation,
                        EndStation = endStation,
                        StartTime = formattedStartTime,
                        EndTime = formattedEndTime,
                        Track = track,
                        Duration = duration,
                        Connections = connections,
                        StopList = new List<DateTimeEntry>()
                    };
                    Console.WriteLine("7");
                    // Process all stops for this trip at once
                    ProcessAllStopsForTrip(tripData, trip);
                    Console.WriteLine("8");
                    // Add the complete trip to our list
                    tripsList.Add(trip);
                        Console.WriteLine($"Trip {i + 1} processed with {trip.StopList.Count} stops");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                }

                return tripsList;
            }

            private static void ProcessAllStopsForTrip(JsonElement tripData, Trip trip)
            {

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

                        if (trip.DateTimeDictionary.ContainsKey(stationName))
                        {
                            Console.WriteLine($"Skipped duplicate stop: {stationName}");
                        }
                        else {
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

                            trip.DateTimeDictionary[stationName] = stopTime;
                            Console.WriteLine($"Added stop: {stationName}");
                        }
                    }
                }
            }

            private void UpdateSuggestions(string query, bool isStartPoint)
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

        [RelayCommand]
        private async Task SaveAsync(Trip trip)
        {
            if (saveButtonText == "Save Trip") 
            {
                saveButtonText = "Unsave Trip";
                // Save the trip to the database
                await _savedTripsTable.SaveTripAsync(trip);  
            }
            else 
            {
                saveButtonText = "Save Trip";
                await _savedTripsTable.RemoveTripAsync(trip);
            }   
        }
        private void HideAllSuggestions()
            {
                IsStartPointSuggestionsVisible = false;
                IsDestinationSuggestionsVisible = false;
            }
        }
    }