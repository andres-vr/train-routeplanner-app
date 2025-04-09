using Routeplanner.ViewModel;

namespace Routeplanner;

public partial class DeparturesPage : ContentPage
{
	public DeparturesPage(DeparturesViewModel viewmodel)
	{
		InitializeComponent();
        BindingContext = viewmodel;
    }
}