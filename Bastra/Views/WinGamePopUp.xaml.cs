using CommunityToolkit.Maui.Views;
using Bastra.ViewModels;
namespace Bastra.Views;

public partial class WinGamePopUp : Popup
{
	public WinGamePopUp(string winnerName, string myName, bool finished)
	{
		InitializeComponent();
		BindingContext = new WinGamePopUpVM(this,winnerName,myName, finished);
	}
}