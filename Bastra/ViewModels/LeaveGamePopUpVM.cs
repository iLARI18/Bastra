using Bastra.Models;
using Bastra.ModelsLogic;
using Bastra.Utilities;
using Bastra.Views;
using System.Windows.Input;

namespace Bastra.ViewModels
{
    public class LeaveGamePopUpVM : ObservableObject
    {
        #region Fields
        private readonly LeaveGamePopUp leaveGamePopUp;
        private string message;
        private readonly Game currentGame;
        private readonly FbData fbd;
        #endregion

        #region Properties
        public string Title => currentGame.IsFull ? "Quit & Lose" : "Quit";
        public string Message
        {
            get => message;
            set
            {
                if (message != value)
                {
                    message = value;
                    OnPropertyChanged(message);
                }
            }
        }
        #endregion

        #region ICommands
        public ICommand LeaveCommand { get; protected set; }
        public ICommand CancelCommand { get; protected set; }
        #endregion

        #region Constructor
        public LeaveGamePopUpVM(LeaveGamePopUp leaveGamePopUp, Game game)
        {
            fbd = new FbData();
            this.leaveGamePopUp = leaveGamePopUp;
            this.currentGame = game;


            LeaveCommand = new Command(Leave);
            CancelCommand = new Command(Cancel);

            Message = DisplayMessage();
        }
        #endregion

        #region Functions
        /// <summary>
        /// Displays a confirmation message when the player attempts to quit the game.
        /// The message varies depending on whether the game has started. If the game is full,
        /// quitting is considered a technical loss, otherwise, it's just a regular quit prompt.
        /// </summary>
        /// <returns>A string containing the message to be displayed to the player.</returns>
        private string DisplayMessage()
        {
            bool isStarted = currentGame.IsFull;
            string result = string.Empty;
            if (isStarted)
                result = "Quiting the game is considered a technical loss!\nAre you sure you want to quit?";
            else
                result = "Are you sure you want to quit?";
            return result;
        }

        /// <summary>
        /// Closes the "Leave Game" popup without performing any action.
        /// This method is used when the player cancels the quit action.
        /// </summary>
        private void Cancel()
        {
            leaveGamePopUp.Close();
        }

        /// <summary>
        /// Handles the player's decision to leave the game. The method updates the game state by
        /// setting the winner's name to the player leaving and updating Firebase. It also resets
        /// the win streak and navigates back to the previous page.
        /// </summary>
        private void Leave()
        {
            Preferences.Set(Constants.WinStreakKey, 0);
            string leavingPlayerName = currentGame.HostName;
            if (currentGame.MyPlayerType == GameModel.PlayerType.Host)
                leavingPlayerName = currentGame.GuestName;
            currentGame.WinnerName = leavingPlayerName;

            fbd.UpdateField("Games", currentGame.Id, "WinnerName", leavingPlayerName);
            leaveGamePopUp.Close();
            Shell.Current.Navigation.PopAsync();
        }
        #endregion
    }
}
