using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Model;
using Routeplanner.Services.Database;
using System.Collections.ObjectModel;
using Routeplanner.ViewModel;

namespace Routeplanner.ViewModel
{
    public partial class SavedViewModel : ObservableObject
    {
        private readonly SavedTripsTable _tripsTable;
        private readonly SavedDeparturesTable _departuresTable;

        [ObservableProperty]
        private string saveButtonText = "Unsave Trip";

        public ObservableCollection<Trip> _Trips { get; } = new();

        public ObservableCollection<Departure> _Departures { get; } = new();

        public SavedViewModel(SavedTripsTable tripsTable, SavedDeparturesTable departuresTable)
        {
            _tripsTable = tripsTable;
            _departuresTable = departuresTable;
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            await Task.Run(GetTripsFromDB);
            await Task.Run(GetDeparturesFromDB);
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

        [RelayCommand]
        private async Task UnsaveAsync(Departure departure)
        {
            if (saveButtonText == "Unsave Departure")
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _departuresTable.RemoveDepartureAsync(departure);
                    _Departures.Remove(departure);
                });
            }
        }

        /*private async Task RemoveTripFromDB(Trip trip)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _tripsTable.RemoveTripAsync(trip);
                _Trips.Remove(trip);
            });
        }*/

        [RelayCommand]
        private async void GoToTripAsync(Trip selectedItem)
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
        private async void GoToDepartureAsync(Departure selectedItem)
        {
            if (selectedItem == null) return;

            var viewModel = new DepartureDetailsViewModel(selectedItem);
            var page = new DepartureDetailsPage(viewModel)
            {
                BindingContext = viewModel
            };

            await Application.Current.MainPage.Navigation.PushAsync(page);
        }
    }
}
