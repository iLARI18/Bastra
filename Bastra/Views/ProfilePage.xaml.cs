using Bastra.ViewModels;

namespace Bastra.Views;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
		BindingContext = new ProfilePageVM();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = new ProfilePageVM();
    }
}