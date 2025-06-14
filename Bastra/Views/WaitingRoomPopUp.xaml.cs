using CommunityToolkit.Maui.Views;
using Bastra.ModelsLogic;
using Bastra.ViewModels;

namespace Bastra.Views;

public partial class WaitingRoomPopUp : Popup
{
    public WaitingRoomPopUp(Game game, string currentPlayerName)
    {
        InitializeComponent();
        BindingContext = new WaitingRoomPopUpVM(game, currentPlayerName, this);
    }
}