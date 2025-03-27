using SQLite;

namespace Routeplanner.Model
{
    public class Station
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
    }
}
