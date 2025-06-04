using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Routeplanner.ViewModel;
using System.Globalization;

namespace Routeplanner
{
    public partial class DepartureDetailsPage : ContentPage
    {
        private DepartureDetailsViewModel _viewmodel;

        public DepartureDetailsPage(DepartureDetailsViewModel viewmodel)
        {
            InitializeComponent();
            BindingContext = viewmodel;
            _viewmodel = viewmodel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var netherlandsPosition = new Location(52.1326, 5.2913);
            var mapSpan = MapSpan.FromCenterAndRadius(netherlandsPosition, Distance.FromKilometers(150));

            var coords = _viewmodel.Departure.coords;

            Polyline polyline = new Polyline
            {
                StrokeWidth = 5,
                StrokeColor = Colors.Red,
            };

            foreach (var coord in coords)
            {
                var latString = coord[1].ToString().Replace(",", ".");
                var lonString = coord[0].ToString().Replace(",", ".");

                if (double.TryParse(latString, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) &&
                    double.TryParse(lonString, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon))
                {
                    polyline.Geopath.Add(new Location(lat, lon));
                    Console.WriteLine($"Added point: {lat}, {lon}");
                }
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DepartureMap.MoveToRegion(mapSpan);
                DepartureMap.MapElements.Add(polyline);
            });
        }
    }
}