using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Routeplanner.Model
{
    [Table("Departures")]
    public class Departure
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Time { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string TrainType { get; set; }
        public string Track { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<DepartureStop> Stops { get; set; } = new List<DepartureStop>();
    }

    [Table("DepartureStops")]
    public class DepartureStop
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Departure))]
        public int DepartureId { get; set; }

        public string StopName { get; set; }
    }
}