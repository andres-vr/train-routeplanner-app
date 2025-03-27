using Routeplanner.ViewModel;

namespace Routeplanner
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainPageViewModel viewmodel)
        {
            InitializeComponent();
            BindingContext = viewmodel;
        }
    }

}
