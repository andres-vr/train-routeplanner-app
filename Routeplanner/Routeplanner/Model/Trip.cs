using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace Routeplanner.Model
{
    [Table("Trips")]
    public class Trip : ObservableObject
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
        public List<Stop> Stops { get; set; } = new();

        private string _saveButtonText = "Save Trip";

        [Ignore]
        public string SaveButtonText
        {
            get => _saveButtonText;
            set => SetProperty(ref _saveButtonText, value);
        }
    }
}
