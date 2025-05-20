using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Routeplanner.Services.Departures
{
    public interface IDepartureService
    {
        Task<string> FetchDeparturesAsync(APIParameters parameters);

        Task<string> FetchMapDataAsync(string codes);

        List<Departure> ExtractDeparturesFromApiResponse(JsonDocument responseData, string currentStation);

        void ProcessRouteStationsForDeparture(JsonElement departureData, Departure departure, string currentStation);
    }
}
