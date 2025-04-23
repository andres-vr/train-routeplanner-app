using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<DateTimeEntry> StopList { get; set; } = new List<DateTimeEntry>();

        [Ignore]
        public Dictionary<string, DateTime> DateTimeDictionary
        {
            get => StopList?.ToDictionary(e => e.Key, e => e.Time) ?? new Dictionary<string, DateTime>();
            set
            {
                if (StopList == null)
                    StopList = new List<DateTimeEntry>();
                else
                    StopList.Clear();

                if (value != null)
                {
                    foreach (var pair in value)
                    {
                        StopList.Add(new DateTimeEntry
                        {
                            Key = pair.Key,
                            Time = pair.Value
                        });
                    }
                }
            }
        }
    }

    // Class to store dictionary entries
    [Table("DateTimeEntries")]
    public class DateTimeEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Trip))]
        public int MainClassId { get; set; }

        public string Key { get; set; }

        public DateTime Time { get; set; }
    }
}
