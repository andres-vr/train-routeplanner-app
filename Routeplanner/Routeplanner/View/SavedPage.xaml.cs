using Routeplanner.ViewModel;

namespace Routeplanner;

public partial class SavedPage : ContentPage
{
	public SavedPage(SavedViewModel viewmodel)
	{
		InitializeComponent();
        BindingContext = viewmodel;
    }
}