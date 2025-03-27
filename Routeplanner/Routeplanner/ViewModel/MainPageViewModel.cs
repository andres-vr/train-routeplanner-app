using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Routeplanner.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
        {
            [ObservableProperty]
            private string _StartPoint;

            [ObservableProperty]
            private string _Destination;

            [ObservableProperty]
            private TimeSpan _SelectedTime;

            [ObservableProperty]
            private DateTime _MinDate;

            [ObservableProperty]
            private DateTime _MaxDate;

            [ObservableProperty]
            private DateTime _SelectedDate;

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
            public void TextComplete()
            {

            }
        }
    }
