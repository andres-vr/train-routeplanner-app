using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.Model
{
    public class APIParameters
    {
        public string fromStation { get; set; }
        public string toStation { get; set; }
        public DateTime selectedDate { get; set; }
        public TimeSpan selectedTime { get; set; }
        //public string SelectedType { get; set; }
    }
}
