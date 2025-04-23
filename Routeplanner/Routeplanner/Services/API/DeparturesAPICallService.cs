using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Routeplanner.Services.API
{
    public class DeparturesAPICallService : IAPICallService
    {
        private readonly HttpClient _client;

        public DeparturesAPICallService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "68ba61bbc3914b5cadb8a0484598d313");
        }

        public async Task<string> MakeCallAsync(APIParameters parameters)
        {
            var baseUrl = "https://gateway.apiportal.ns.nl/reisinformatie-api/api/v2/departures";
            try
            {
                var queryParams = new Dictionary<string, string>
            {
                { "station", parameters.FromStation }
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
