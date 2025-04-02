using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Services;
using System.Linq.Expressions;

namespace Routeplanner.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
        {
            private readonly ITripService _tripService;

            [ObservableProperty]
            private string _startPoint;

            partial void OnStartPointChanged(string value)
            {
            
            }

            [ObservableProperty]
            private string _Destination;

            partial void OnDestinationChanged(string value)
            {
                
            }

            [ObservableProperty]
            private TimeSpan _SelectedTime;

            [ObservableProperty]
            private DateTime _MinDate;

            [ObservableProperty]
            private DateTime _MaxDate;

            [ObservableProperty]
            private DateTime _SelectedDate;

            [ObservableProperty]
            private string _SelectedType;

            public MainPageViewModel(ITripService tripService)
            {
                _tripService = tripService;

                // Set default date range
                _MinDate = DateTime.Today;
                    _MaxDate = DateTime.Today.AddYears(1);
                    _SelectedDate = DateTime.Today;
                    _SelectedTime = DateTime.Now.TimeOfDay;
                }

            [RelayCommand]
            public void Completed()
            {
            
            }

            [RelayCommand]
            public async void Search()
            {
                Console.WriteLine("Seach pressed");
                if (string.IsNullOrWhiteSpace(StartPoint) || string.IsNullOrWhiteSpace(Destination))
                {
                    Console.WriteLine("Please enter valid station names.");
                    return;
                }
                try
                {
                    string response = await _tripService.FetchTripsAsync(StartPoint, Destination, SelectedDate, SelectedTime/*, SelectedType*/);
                    Console.WriteLine("API Response:");
                    Console.WriteLine(response);
                }
                catch (Exception ex) { 
            
                }
        }
    }
}
