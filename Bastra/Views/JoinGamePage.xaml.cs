using Bastra.ModelsLogic;
using Bastra.ViewModels;

namespace Bastra.Views
{
    public partial class JoinGamePage : ContentPage
    {
        private readonly JoinGamePageVM jgpvm;

        public JoinGamePage(string myName)
        {
            InitializeComponent();
            jgpvm = new JoinGamePageVM(this, myName); // Set the BindingContext
            BindingContext = jgpvm;
            lvGames.ItemTapped += OnItemTapped;
        }
        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            Game game = (Game)e.Item;
            jgpvm.JoinGame(game);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            jgpvm.SelectedScore = 0;
            jgpvm.AddSnapshotListener();
        }

        protected override void OnDisappearing()
        {
            jgpvm.RemoveSnapshotListener();
            base.OnDisappearing();
        }
    }
}
