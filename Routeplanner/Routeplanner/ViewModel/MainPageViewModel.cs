using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Services;

namespace Routeplanner.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
        {
            ITripService tripService;

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

            public MainPageViewModel()
                {
                    // Set default date range
                    _MinDate = DateTime.Today;
                    _MaxDate = DateTime.Today.AddYears(1);
                    _SelectedDate = DateTime.Today;
                    _SelectedTime = DateTime.Now.TimeOfDay;
                }

            [RelayCommand]
            public void TextChange()
            {

            }

            [RelayCommand]
            public void Completed()
            {
            
            }

        [RelayCommand]
            public void Search()
            {

            }
    }
}
