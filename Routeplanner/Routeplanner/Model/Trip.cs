using SQLite;

namespace Routeplanner.Model
{
    [Table("Trips")]
    public class Trip
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string StartStation { get; set; }
        public string EndStation { get; set; }
        public string Track { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int Connections { get; set; }

        [Ignore]
        public List<Stop> Stops { get; set; } = new List<Stop>();

        public class Stop
        {
            public int Id { get; set; }
            public int TripId { get; set; } // Foreign key to Trip

            // Other Stop properties
            public string Station { get; set; }
            public DateTime Time { get; set; }
        }
    }
}
