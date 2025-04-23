using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.Model
{
    public class APIParameters
    {
        public string FromStation { get; set; }
        public string ToStation { get; set; }
        public DateTime SelectedDate { get; set; }
        public TimeSpan SelectedTime { get; set; }
        //public string SelectedType { get; set; }
    }
}
