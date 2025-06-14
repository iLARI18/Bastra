using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bastra.ViewModels;
using Bastra.ModelsLogic;


namespace Bastra.Views
{
    public partial class GamePage : ContentPage
    {
        private readonly GamePageVM gpvm;

        public GamePage(Game game, string myName)
        {
            InitializeComponent();
            gpvm = new GamePageVM(this, game, grdMyCards, grdTableCards, scrlMyCards, myName);
            BindingContext = gpvm;

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            gpvm.AddSnapshotListener();
        }

        protected override void OnDisappearing()
        {
            gpvm.RemoveSnapshotListener();
            gpvm.DeleteDocument();
            base.OnDisappearing();
        }
    }
}
