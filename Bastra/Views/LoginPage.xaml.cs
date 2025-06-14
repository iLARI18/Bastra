using Bastra.ViewModels;
namespace Bastra.Views;
public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
        InitializeComponent();
        BindingContext = new LoginPageVM();
    }
}