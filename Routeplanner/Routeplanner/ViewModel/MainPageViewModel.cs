using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Routeplanner.Model;
using Routeplanner.Services;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Routeplanner.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly ITripService _tripService;
        private readonly SqliteDatabaseService _databaseService;

        private List<string> _stationCache = new();

        [ObservableProperty]
        private string _startPoint;

        [ObservableProperty]
        private string _destination;

        [ObservableProperty]
        private TimeSpan _selectedTime;

        [ObservableProperty]
        private DateTime _selectedDate;

        [ObservableProperty]
        private DateTime _MinDate;

        [ObservableProperty]
        private DateTime _MaxDate;
        
        [ObservableProperty]
        private string _selectedType;

        [ObservableProperty]
        private ObservableCollection<string> _startPointSuggestions = new();

        [ObservableProperty]
        private ObservableCollection<string> _destinationSuggestions = new();

        [ObservableProperty]
        private bool _isStartPointSuggestionsVisible;

        [ObservableProperty]
        private bool _isDestinationSuggestionsVisible;

        public MainPageViewModel(ITripService tripService, SqliteDatabaseService databaseService)
        {
            _tripService = tripService;
            _databaseService = databaseService;

            // Set default date range
            _MinDate = DateTime.Today;
            _MaxDate = DateTime.Today.AddYears(1);
            SelectedDate = DateTime.Today;
            SelectedTime = DateTime.Now.TimeOfDay;

            Task.Run(CacheStationsAsync);
        }

        private async Task CacheStationsAsync()
        {
            var stations = await _databaseService.GetAllStataions(); 
            _stationCache = stations.Select(s => s.name).ToList();
        }

        // Handlers for text changes
        partial void OnStartPointChanged(string value) =>
            UpdateSuggestions(value, true);

        partial void OnDestinationChanged(string value) =>
            UpdateSuggestions(value, false);

        [RelayCommand]
        private void Completed() => HideAllSuggestions();

        [RelayCommand]
        private void SelectStartPoint(string selectedItem)
        {
            StartPoint = selectedItem;
            IsStartPointSuggestionsVisible = false;
        }

        [RelayCommand]
        private void SelectDestination(string selectedItem)
        {
            Destination = selectedItem;
            IsDestinationSuggestionsVisible = false;
        }

        [RelayCommand]
        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(StartPoint) || string.IsNullOrWhiteSpace(Destination))
            {
                Console.WriteLine("Please enter valid station names.");
                return;
            }

            try
            {
                var parameters = new APIParameters
                {
                    fromStation = StartPoint,
                    toStation = Destination,
                    selectedDate = SelectedDate,
                    selectedTime = SelectedTime
                };

                string response = await _tripService.FetchTripsAsync(parameters);
                Console.WriteLine($"API Response: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async void UpdateSuggestions(string query, bool isStartPoint)
        {
            // use station cache to update suggestions
            var results = _stationCache
               .Where(s => s.StartsWith(query, StringComparison.OrdinalIgnoreCase)) // Faster lookup
               .Take(10) // Limit the number of suggestions
               .ToList();
            if (isStartPoint)
            {
                StartPointSuggestions.Clear();
                foreach (var item in results)
                    StartPointSuggestions.Add(item);

                // toon suggesties als er resultaten zijn en de query niet leeg is
                IsStartPointSuggestionsVisible = results.Any() && !string.IsNullOrEmpty(query);
            }
            else
            {
                DestinationSuggestions.Clear();
                foreach (var item in results)
                    DestinationSuggestions.Add(item);

                // toon suggesties als er resultaten zijn en de query niet leeg is
                IsDestinationSuggestionsVisible = results.Any() && !string.IsNullOrEmpty(query);
            }
        }

        private void HideAllSuggestions()
        {
            IsStartPointSuggestionsVisible = false;
            IsDestinationSuggestionsVisible = false;
        }
    }
}