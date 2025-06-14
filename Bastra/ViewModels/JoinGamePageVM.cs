using Bastra.Models;
using Bastra.ModelsLogic;
using Bastra.Utilities;
using Bastra.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Bastra.ViewModels
{
    public class JoinGamePageVM : ObservableObject
    {
        #region Fields
        private readonly JoinGamePage jgp;
        private readonly Games games;
        private string myName = string.Empty;
        private int selectedScore;
        #endregion

        #region ICommands
        public ICommand AddGameCommand { get; protected set; }
        public ICommand BackCommand { get; protected set; }
        public ICommand OpenSelectionCommand { get; protected set; }
        #endregion

        #region Properties
        public ObservableCollection<Game> GamesList => games.GamesList;
        public int SelectedScore
        {
            get => selectedScore;
            set
            {
                if (selectedScore != value)
                {
                    selectedScore = value;
                    OnPropertyChanged(nameof(SelectedScore));
                    OnPropertyChanged(nameof(TargetPointsDisplay));
                }
            }
        }
        public string TargetPointsDisplay => SelectedScore > 0 ? $"TARGET {SelectedScore}" : "TARGET";
        #endregion

        #region Constructor
        public JoinGamePageVM(JoinGamePage page, string myName)
        {
            this.jgp = page;
            this.myName = myName;
            games = new Games(myName);
            games.GameCreated += OnGameCreated;
            AddGameCommand = new Command(AddGame);
            OpenSelectionCommand = new Command(OpenSelection);
            BackCommand = new Command(Back);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Opens a selection dialog for the user to choose a score target. The user can choose from predefined options
        /// or enter a custom score. If a custom score is entered, it must be a valid integer between 10 and 999.
        /// The selected score is then assigned to <see cref="SelectedScore"/>.
        /// </summary>
        private async void OpenSelection()
        {
            string[] scoreOptions =
                { "1 Point", "50 Points", "101 Points", "150 Points", "201 Points", "Custom" };

            string selected = await Application.Current.MainPage.DisplayActionSheet(
                                  "Select Score Target",
                                  "Cancel",
                                  null,
                                  scoreOptions);

            if (!string.IsNullOrEmpty(selected) && selected != "Cancel")
            {
                if (selected == "Custom")
                {
                    string input = await Application.Current.MainPage.DisplayPromptAsync(
                                       "Custom Target",
                                       "Enter a target score",
                                       "OK",
                                       "Cancel",
                                       keyboard: Microsoft.Maui.Keyboard.Numeric);

                    if (!string.IsNullOrEmpty(input))
                    {
                        if (int.TryParse(input, out int customScore))
                        {
                            if (customScore >= 10 && customScore <= 999)
                            {
                                SelectedScore = customScore;
                            }
                            else
                            {
                                await Application.Current.MainPage.DisplayAlert(
                                    "Invalid Input",
                                    "Target must be between 10 and 999.",
                                    "OK");
                            }
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Invalid Input",
                                "Target must be an integer.",
                                "OK");
                        }
                    }
                }
                else
                {
                    if (int.TryParse(selected.Split(' ')[0], out int score))
                    {
                        SelectedScore = score;
                    }
                }
            }

            return;
        }


        /// <summary>
        /// Navigates back to the previous page in the navigation stack.
        /// </summary>
        private void Back()
        {
            Shell.Current.Navigation.PopAsync();
        }

        /// <summary>
        /// Adds a new game to the list of games. If a valid score target has been selected, it is used for the game,
        /// otherwise the default target score is used.
        /// </summary>
        private void AddGame()
        {
            if (SelectedScore > 0)
                games.AddGame(SelectedScore);
            else
                games.AddGame(Constants.DefaultTarget);
        }
        /// <summary>
        /// Handles the event when a game is created. It navigates to the new game page using the created game object.
        /// The game page is displayed on the main thread.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the game object.</param>
        /// <param name="e">Event arguments containing the event data.</param>
        private void OnGameCreated(object sender, EventArgs e)
        {
            Game game = (Game)sender;
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                Shell.Current.Navigation.PushAsync(new GamePage(game, myName));
            });
        }
        /// <summary>
        /// Joins an existing game by calling the <see cref="games.JoinGame"/> method with the specified game object.
        /// </summary>
        /// <param name="game">The game to join.</param>
        public void JoinGame(Game game)
        {
            games.JoinGame(game);
        }
        /// <summary>
        /// Adds a snapshot listener to listen for changes in the game data in real time.
        /// </summary>
        public void AddSnapshotListener()
        {
            games.AddSnapshotListener();
        }
        /// <summary>
        /// Removes the snapshot listener, stopping real-time updates for game data.
        /// </summary>
        public void RemoveSnapshotListener()
        {
            games.RemoveSnapshotListener();
        }
        #endregion
    }
}
