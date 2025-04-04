using Routeplanner.ViewModel;

namespace Routeplanner
{
    public partial class PlanPage : ContentPage
    {

        public PlanPage(PlanViewModel viewmodel)
        {
            InitializeComponent();
            BindingContext = viewmodel;
        }
    }

}
