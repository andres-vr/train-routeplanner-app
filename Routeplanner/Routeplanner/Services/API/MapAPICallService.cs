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
    public class MapAPICalLService
    {
        private readonly HttpClient _client;

        public MapAPICalLService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "68ba61bbc3914b5cadb8a0484598d313");
        }

        public async Task<string> MakeCallAsync(string codes)
        {
            var baseUrl = "https://gateway.apiportal.ns.nl/Spoorkaart-API/api/v1/traject?stations=";
            try
            {
                var queryParams = codes;
                Console.Write(baseUrl + "" + queryParams);

                var uri = $"{baseUrl}{queryParams}";

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