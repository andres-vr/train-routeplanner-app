using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.Model
{
    public class Departure
    {
        public string Time { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string TrainType { get; set; }
        public string Track { get; set; }
        public List<string> Stops { get; set; }
    }
}
