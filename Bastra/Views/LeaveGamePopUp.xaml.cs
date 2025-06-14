using Bastra.ModelsLogic;
using Bastra.ViewModels;
using CommunityToolkit.Maui.Views;

namespace Bastra.Views
{
    public partial class LeaveGamePopUp : Popup
    {
        public LeaveGamePopUp(Game game)
        {
            InitializeComponent();
            BindingContext = new LeaveGamePopUpVM(this, game);
        }
    }
}

