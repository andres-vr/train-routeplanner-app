using Routeplanner.ViewModel;

namespace Routeplanner;

public partial class SavedPage : ContentPage
{
	public SavedPage(SavedPage viewmodel)
	{
		InitializeComponent();
        BindingContext = viewmodel;
    }
}