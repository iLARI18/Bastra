using Bastra.ModelsLogic;
using Plugin.CloudFirestore;
using System.Collections.ObjectModel;

namespace Bastra.Utilities
{
    public class Games
    {
        private IListenerRegistration ilr;
        private readonly string myName;
        private readonly FbData fbd;

        public event EventHandler GameCreated;

        public ObservableCollection<Game> GamesList { get; set; } = [];

        public Games(string myName)
        {
            fbd = new FbData();
            this.myName = myName;
        }
        private void OnChange(IQuerySnapshot snapshot, Exception error)
        {
            
            fbd.GetDocumentsWhereEqualTo(Constants.collectionName, "IsFull", false, OnComplete);
        }

        private void OnComplete(IQuerySnapshot qs)
        {
            GamesList.Clear();
            foreach (IDocumentSnapshot ds in qs.Documents)
            {
                Game game = ds.ToObject<Game>();
                game.Id = ds.Id;
                GamesList.Add(game);
            }
        }

        public void AddSnapshotListener()
        {
            ilr = fbd.AddSnapshotListener(Constants.collectionName, OnChange);
        }
        public void RemoveSnapshotListener()
        {
            ilr?.Remove();
        }
        public void AddGame(int pointsTarget)
        {
            Game game = new();
            game.SetScore(pointsTarget);   
            game.GameCreated += OnGameCreated;
            game.SetDocument();

            
        }
        public void JoinGame(Game game)
        {
            game.GameCreated += OnGameCreated;
            game.JoinGame();       
        }

        private void OnGameCreated(object sender, EventArgs e)
        {
            GameCreated.Invoke(sender, EventArgs.Empty);
        }

    }
}
