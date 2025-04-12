using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Model;
using Routeplanner.Services.Database;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Routeplanner.ViewModel
{
    public partial class SavedViewModel : ObservableObject
    {
        private readonly SavedTripsTable _tripsTable;
        private readonly SavedDeparturesTable _departuresTable;

        public ObservableCollection<Trip> _Trips { get; } = new();

        public ObservableCollection<Departure> _Departures { get; } = new();

        public SavedViewModel(SavedTripsTable tripsTable, SavedDeparturesTable departuresTable)
        {
            _tripsTable = tripsTable;
            _departuresTable = departuresTable;
            Task.Run(GetTripsFromDB);
            Task.Run(GetDeparturesFromDB);
            Task.Run(CreateDepartureInDB);
        }

        private async Task GetTripsFromDB()
        {
            var trips = await _tripsTable.GetAllTrips();
            foreach (var trip in trips)
            {
                _Trips.Add(trip);
            }
        }

        private async Task GetDeparturesFromDB()
        {
            var departures = await _departuresTable.GetAllDepartures();
            foreach (var departure in departures)
            {
                _Departures.Add(departure);
            }
        }

        private async Task CreateDepartureInDB()
        {
            MainThread.BeginInvokeOnMainThread(async () => {
                Departure departure;
                departure = new Departure
                {
                    Time = "12:00",
                    Origin = "Amsterdam",
                    Destination = "Rotterdam",
                    TrainType = "IC",
                    Track = "1",
                    Stops = new List<string> { "Utrecht", "Den Haag" }
                };
                await _departuresTable.SaveDepartureAsync(departure);
                Console.Write("DEPARTURE ADDED");
                _Departures.Add(departure);
            });
        }

        [RelayCommand]
        private void GoToTrip(Trip selectedItem)
        {
            Console.WriteLine($"Selected trip: {selectedItem.startStation} to {selectedItem.endStation}");
        }

        [RelayCommand]
        private void GoToDeparture(Departure selectedItem)
        {
            Console.WriteLine($"Selected trip: {selectedItem.Origin}");
        }
    }
}
