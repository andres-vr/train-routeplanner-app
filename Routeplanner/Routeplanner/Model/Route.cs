using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.Model
{
    public class Route
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string fromStation { get; set; }
        public string toStation { get; set; }
    }
}
