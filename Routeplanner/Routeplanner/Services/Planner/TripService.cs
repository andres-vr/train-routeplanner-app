using Routeplanner.Model;
using Routeplanner.Services.API;
using System.Text.Json;
using System.Threading.Tasks;

namespace Routeplanner.Services.Planner
{
    public class TripService : ITripService
    {
        private readonly TripsAPICallService _apiCallService;
        private readonly MapAPICalLService _mapAPICalLService;
        private StationTable _stations;

        public TripService(TripsAPICallService apiCallService, MapAPICalLService mapApiService, StationTable stations)
        {
            _apiCallService = apiCallService;
            _mapAPICalLService = mapApiService;
            _stations = stations;
        }

        public async Task<string> FetchTripsAsync(APIParameters parameters)
        {
            return await _apiCallService.MakeCallAsync(parameters);
        }

        public async Task<string> FetchMapDataAsync(string codes)
        {
            return await _mapAPICalLService.MakeCallAsync(codes);
        }

        public Task<List<Trip>> GetTrips()
        {
            throw new NotImplementedException();
        }

        public List<Trip> ExtractTripsFromApiResponse(JsonDocument responseData)
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

                    string startStation = "";
                    string endStation = "";
                    string formattedStartTime = "";
                    string formattedEndTime = "";
                    string track = "";
                    TimeSpan duration = TimeSpan.Zero;
                    int connections = 0;

                    if (tripData.TryGetProperty("legs", out JsonElement legs) && legs.GetArrayLength() > 0)
                    {
                        var firstLeg = legs[0];
                        var lastLeg = legs[legs.GetArrayLength() - 1];

                        if (firstLeg.TryGetProperty("origin", out JsonElement origin))
                        {
                            origin.TryGetProperty("name", out JsonElement originName);
                            startStation = originName.GetString() ?? "";

                            if (origin.TryGetProperty("actualDateTime", out JsonElement startTimeElement))
                            {
                                if (DateTime.TryParse(startTimeElement.GetString(), out DateTime startTime))
                                {
                                    formattedStartTime = startTime.ToString("HH:mm");
                                }
                            }

                            if (origin.TryGetProperty("actualTrack", out JsonElement trackElement))
                            {
                                track = trackElement.GetString() ?? "";
                            }
                            else if (origin.TryGetProperty("plannedTrack", out JsonElement plannedTrackElement))
                            {
                                track = plannedTrackElement.GetString() ?? "";
                            }
                        }

                        if (lastLeg.TryGetProperty("destination", out JsonElement destination))
                        {
                            destination.TryGetProperty("name", out JsonElement destName);
                            endStation = destName.GetString() ?? "";

                            if (destination.TryGetProperty("actualDateTime", out JsonElement endTimeElement))
                            {
                                if (DateTime.TryParse(endTimeElement.GetString(), out DateTime endTime))
                                {
                                    formattedEndTime = endTime.ToString("HH:mm");
                                }
                            }
                        }
                    }

                    if (tripData.TryGetProperty("actualDurationInMinutes", out JsonElement durationElement))
                    {
                        duration = TimeSpan.FromMinutes(durationElement.GetInt32());
                    }
                    else if (tripData.TryGetProperty("plannedDurationInMinutes", out JsonElement plannedDurationElement))
                    {
                        duration = TimeSpan.FromMinutes(plannedDurationElement.GetInt32());
                    }

                    if (tripData.TryGetProperty("transfers", out JsonElement transfersElement))
                    {
                        connections = transfersElement.GetInt32();
                    }

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
                        Stops = new List<Stop>()
                    };
                    // Process all stops for this trip at once
                    ProcessAllStopsForTrip(tripData, trip);
                    // Add the complete trip to our list
                    tripsList.Add(trip);
                    Console.WriteLine($"Trip {i + 1} processed with {trip.Stops.Count} stops");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }

            return tripsList;
        }

        public async void ProcessAllStopsForTrip(JsonElement tripData, Trip trip)
        {
            List<string> Stations = new List<string>();

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
                    Stop stopItem = new Stop
                    {
                        Station = stationName,
                        Time = stopTime
                    };

                    trip.Stops.Add(stopItem);
                    Stations.Add(stationName);
                    string[] stations = Stations.ToArray();
                    var codesString = _stations.NamesToCodes(stations).ToString();
                    string response = await FetchMapDataAsync(codesString);
                    JsonDocument mapData = JsonDocument.Parse(response);
                    Console.WriteLine(mapData.ToString());
                    Console.WriteLine($"Added stop: {stationName}");
                }
            }
        }
    }
}