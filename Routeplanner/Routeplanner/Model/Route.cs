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
        public int Id { get; set; }
        public string FromStation { get; set; }
        public string ToStation { get; set; }
    }
}
