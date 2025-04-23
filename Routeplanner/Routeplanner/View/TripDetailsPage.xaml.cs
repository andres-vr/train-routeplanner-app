using Routeplanner.ViewModel;

namespace Routeplanner
{
    public partial class TripDetailsPage : ContentPage
    {

        public TripDetailsPage(TripDetailsViewModel viewmodel)
        {
            InitializeComponent();
            BindingContext = viewmodel;
        }
    }
}