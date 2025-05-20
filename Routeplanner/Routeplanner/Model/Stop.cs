using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.Model
{
    public class Stop
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public string Station { get; set; }
        public DateTime Time { get; set; }
    }
}
