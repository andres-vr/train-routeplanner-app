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

        public string startStation { get; set; }
        public string endStation { get; set; }
        public string track { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string duration { get; set; }
        public int connections { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<DateTimeEntry> stopList { get; set; } = new List<DateTimeEntry>();

        [Ignore]
        public Dictionary<string, DateTime> DateTimeDictionary
        {
            get => stopList?.ToDictionary(e => e.Key, e => e.DateValue) ?? new Dictionary<string, DateTime>();
            set
            {
                if (stopList == null)
                    stopList = new List<DateTimeEntry>();
                else
                    stopList.Clear();

                if (value != null)
                {
                    foreach (var pair in value)
                    {
                        stopList.Add(new DateTimeEntry
                        {
                            Key = pair.Key,
                            DateValue = pair.Value
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

        public DateTime DateValue { get; set; }
    }
}
