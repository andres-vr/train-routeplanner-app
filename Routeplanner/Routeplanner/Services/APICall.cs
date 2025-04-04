﻿using Routeplanner.Model;
using System.Net.Http.Headers;
using System.Web;

namespace Routeplanner.Services
{
    public class APICallService
    {
        private readonly HttpClient _client;

        public APICallService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "68ba61bbc3914b5cadb8a0484598d313");
        }

        public async Task<string> GetTripsAsync(APIParameters parameters)
        {
            var baseUrl = "https://gateway.apiportal.ns.nl/reisinformatie-api/api/v3/trips";
            try
            {
                // Combine them into a single DateTime
                DateTime combinedDateTime = parameters.selectedDate.Date.Add(parameters.selectedTime);

                // Format according to RFC 3339
                string formattedDateTime = combinedDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz");

                var queryParams = new Dictionary<string, string>
            {
                { "fromStation", parameters.fromStation },
                { "toStation", parameters.toStation },
                { "originWalk", "false" },
                { "originBike", "false" },
                { "originCar", "false" },
                { "destinationWalk", "false" },
                { "destinationBike", "false" },
                { "destinationCar", "false" },
                { "dateTime", formattedDateTime },
                { "shorterChange", "false" },
                { "travelAssistance", "false" },
                { "searchForAccessibleTrip", "false" },
                { "localTrainsOnly", "false" },
                { "excludeHighSpeedTrains", "false" },
                { "excludeTrainsWithReservationRequired", "false" },
                { "product", "OVCHIPKAART_ENKELE_REIS" },
                { "discount", "NO_DISCOUNT" },
                { "travelClass", "2" },
                { "passing", "false" },
                { "travelRequestType", "DEFAULT" }
            };

                var queryString = HttpUtility.ParseQueryString(string.Empty);
                foreach (var param in queryParams)
                {
                    queryString[param.Key] = param.Value;
                }

                var uri = $"{baseUrl}?{queryString}";

                Console.WriteLine($"Requesting: {uri}");

                var response = await _client.GetAsync(uri);
                return await response.Content.ReadAsStringAsync();
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return string.Empty;
            }
        }
    }
}