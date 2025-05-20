using Routeplanner.Model;
using Routeplanner.Services.API;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Routeplanner.Services.Departures
{
    public class DepartureService : IDepartureService
    {
        private readonly DeparturesAPICallService _apiCallService;
        private readonly MapAPICalLService _mapAPICalLService;
        private StationTable _stations;

        public DepartureService(DeparturesAPICallService apiCallService, MapAPICalLService mapApiService, StationTable stationTable)
        {
            _apiCallService = apiCallService;
            _mapAPICalLService = mapApiService;
            _stations = stationTable;
        }

        public async Task<string> FetchDeparturesAsync(APIParameters parameters)
        {
            return await _apiCallService.MakeCallAsync(parameters);
        }

        public async Task<string> FetchMapDataAsync(string codes)
        {
            return await _mapAPICalLService.MakeCallAsync(codes);
        }

        public List<Departure> ExtractDeparturesFromApiResponse(JsonDocument responseData, string currentStation)
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

        public async void ProcessRouteStationsForDeparture(JsonElement departureData, Departure departure, string currentStation)
        {
            List<string> Stations = new List<string>();
            // start station
            departure.Stops.Add(new DepartureStop { StopName = currentStation });
            Stations.Add(currentStation);
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
                    Stations.Add(stationName);
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
            Stations.Add(destinationName);
            string[] stations = Stations.ToArray();
            string codesString = await _stations.NamesToCodes(stations);
            string response = await FetchMapDataAsync(codesString);

            JsonDocument mapData = JsonDocument.Parse(response);
            Console.Write(mapData);
            JsonElement mapArray = mapData.RootElement.GetProperty("payload").GetProperty("features")[0];
            Console.WriteLine(mapData.RootElement.GetProperty("payload").GetProperty("features")[0]);
            var element = mapArray.GetProperty("geometry").GetProperty("coordinates");

            int length = element.GetArrayLength();
            Console.WriteLine(length);
            double[][] coordinates = new double[length][]; // Using double for numeric values

            for (int i = 0; i < length; i++)
            {
                JsonElement coordsElement = mapArray.GetProperty("geometry").GetProperty("coordinates")[i];

                double[] coordsArray = new double[2];
                coordsArray[0] = coordsElement[0].GetDouble();
                coordsArray[1] = coordsElement[1].GetDouble();

                coordinates[i] = coordsArray;
            }
            departure.coords = coordinates;
            Console.WriteLine(response);
            Console.WriteLine($"Added destination stop: {destinationName}");
        }
    }
}