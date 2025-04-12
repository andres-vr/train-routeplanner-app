using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.Model
{
    public class Trip
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string startStation { get; set; }
        public string endStation { get; set; }
        public string track { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string duration { get; set; }
        public int connections { get; set; }
        public Dictionary<string, DateTime> stopList { get; set; }
    }
}
