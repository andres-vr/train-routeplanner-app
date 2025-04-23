using Routeplanner.ViewModel;

namespace Routeplanner
{
    public partial class DepartureDetailsPage : ContentPage
    {

        public DepartureDetailsPage(DepartureDetailsViewModel viewmodel)
        {
            InitializeComponent();
            BindingContext = viewmodel;
        }
    }
}