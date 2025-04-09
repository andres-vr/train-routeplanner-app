using Routeplanner.Model;
using Routeplanner.Services.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.Services.Departures
{
    public class DepartureService : IDepartureService
    {
        private readonly DeparturesAPICallService _apiCallService;

        public DepartureService(DeparturesAPICallService apiCallService)
        {
            _apiCallService = apiCallService;
        }

        public async Task<string> FetchDeparturesAsync(APIParameters parameters)
        {
            return await _apiCallService.MakeCallAsync(parameters);
        }
    }
}
