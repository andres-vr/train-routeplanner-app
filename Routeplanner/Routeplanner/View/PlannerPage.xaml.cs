using Routeplanner.ViewModel;

namespace Routeplanner
{
    public partial class PlannerPage : ContentPage
    {

        public PlannerPage(PlannerViewModel viewmodel)
        {
            InitializeComponent();
            BindingContext = viewmodel;
        }
    }

}
