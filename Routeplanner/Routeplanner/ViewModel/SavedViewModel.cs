using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Model;
using Routeplanner.Services.Database;
using System.Collections.ObjectModel;

namespace Routeplanner.ViewModel
{
    public partial class SavedViewModel : ObservableObject
    {
        private readonly SavedTripsTable _tripsTable;
        private readonly SavedDeparturesTable _departuresTable;

        public ObservableCollection<Trip> Trips { get; } = new();

        public ObservableCollection<Departure> Departures { get; } = new();

        public SavedViewModel(SavedTripsTable tripsTable, SavedDeparturesTable departuresTable)
        {
            _tripsTable = tripsTable;
            _departuresTable = departuresTable;
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            if (Trips.Count == 0)
            {
                await Task.Run(GetTripsFromDB);
            }
            if (Departures.Count == 0)
            {
                await Task.Run(GetDeparturesFromDB);
            }
        }

        private async Task GetTripsFromDB()
        {
            var trips = await _tripsTable.GetAllTrips();
            foreach (var trip in trips)
            {
                Trips.Add(trip);
            }
        }

        private async Task GetDeparturesFromDB()
        {
            var departures = await _departuresTable.GetAllDepartures();
            foreach (var departure in departures)
            {
                Departures.Add(departure);
            }
        }

        [RelayCommand]
        private async void Clear()
        {
            Trips.Clear();
            Departures.Clear();
            _tripsTable.RemoveAllTrips();
            _departuresTable.RemoveAllDepartures();
        }

        [RelayCommand]
        private async Task GoToTripAsync(Trip selectedItem)
        {
            if (selectedItem == null) return;

            var viewModel = new TripDetailsViewModel(selectedItem);
            var page = new TripDetailsPage(viewModel)
            {
                BindingContext = viewModel
            };

            await Application.Current.MainPage.Navigation.PushAsync(page);
        }

        [RelayCommand]
        private async Task GoToDepartureAsync(Departure selectedItem)
        {
            if (selectedItem == null) return;

            var viewModel = new DepartureDetailsViewModel(selectedItem);
            var page = new DepartureDetailsPage(viewModel)
            {
                BindingContext = viewModel
            };

            await Application.Current.MainPage.Navigation.PushAsync(page);
        }

        [RelayCommand]
        private async Task UnsaveTripAsync(Trip trip)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _tripsTable.RemoveTripAsync(trip);
                Trips.Remove(trip);
            });
        }

        [RelayCommand]
        private async Task UnsaveDepartureAsync(Departure departure)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _departuresTable.RemoveDepartureAsync(departure);
                Departures.Remove(departure);
            });
        }
    }
}
