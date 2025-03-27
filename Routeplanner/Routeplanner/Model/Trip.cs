using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.Model
{
    public class Trip
    {
        public string startStation { get; set; }
        public string endStation { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public Dictionary<string, DateTime> stopList { get; set; }
    }
}
