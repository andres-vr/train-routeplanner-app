using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.Services.API
{
    public interface IAPICallService
    {
        Task<string> MakeCallAsync(APIParameters parameters);
    }
}
