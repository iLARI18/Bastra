using Bastra.Models;
using Bastra.Utilities;
using Bastra.Views;
using System.Windows.Input;

namespace Bastra.ViewModels
{
    public class WinGamePopUpVM : ObservableObject
    {
        #region Fields
        private readonly string winnerName;
        private readonly string myName;
        private readonly WinGamePopUp winGamePopUp;
        private readonly bool finished;
        #endregion

        #region ICommands
        public ICommand LeaveCommand { get; protected set; }
        #endregion

        #region Properties
        public string GameStatus => "Game Over";
        public string Message
        {
            get
            {
                if (!finished)
                {
                    UpdateWinStreak();
                    return "Your opponent has quit the game.\nYou received a technical victory. 🏆";
                }

                if( myName == winnerName)
                {
                    UpdateWinStreak();
                    return "🎉 Congratulations!\nYou won the game! 🏆";
                }
                else
                {
                    Preferences.Set(Constants.WinStreakKey, 0);
                    return $"😔 You lost this time.\n{winnerName} took the victory.";
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the ViewModel for the win game popup. It sets up the winner's name, the current player's name, 
        /// and whether the game is finished. The constructor also initializes the command for leaving the game.
        /// </summary>
        /// <param name="winGamePopUp">The WinGamePopUp instance associated with this ViewModel.</param>
        /// <param name="winnerName">The name of the winner of the game.</param>
        /// <param name="myName">The current player's name.</param>
        /// <param name="finished">Indicates whether the game has finished.</param>
        public WinGamePopUpVM(WinGamePopUp winGamePopUp, string winnerName, string myName, bool finished)
        {
            this.winGamePopUp = winGamePopUp;
            this.winnerName = winnerName;
            this.myName = myName;
            this.finished = finished;
            LeaveCommand = new Command(Leave);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Handles the player's decision to leave the win game popup. The method navigates back to the previous page 
        /// and closes the win game popup.
        /// </summary>
        private void Leave()
        {
            Shell.Current.Navigation.PopAsync();
            winGamePopUp.Close();
        }

        /// <summary>
        /// Updates the player's win streak. It increments the current win streak and updates the preferences accordingly.
        /// If the current win streak is greater than the longest win streak, it updates the longest win streak as well.
        /// </summary>
        private void UpdateWinStreak()
        {
            int currentWinStreak = Preferences.Get(Constants.WinStreakKey, 0);
            Preferences.Set(Constants.WinStreakKey, currentWinStreak + 1);

            if (Preferences.Get(Constants.LongestWinStreakKey, 0) < Preferences.Get(Constants.WinStreakKey, 0))
                Preferences.Set(Constants.LongestWinStreakKey, currentWinStreak + 1);
        }

        #endregion
    }
}
