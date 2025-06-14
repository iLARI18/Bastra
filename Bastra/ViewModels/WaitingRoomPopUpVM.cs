using Bastra.Models;
using Bastra.ModelsLogic;
using Bastra.Utilities;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Input;

namespace Bastra.ViewModels
{
    public class WaitingRoomPopUpVM : ObservableObject
    {
        #region Fields
        private readonly Game game;
        private readonly Popup popup;
        private readonly GameTimer gameTimer;
        private readonly FbData fbd;
        private bool countdownStarted = false;
        private bool IsPopupClosed = false;
        private const double countdownTime = 6.0;
        private GameStatusType gameStatus = GameStatusType.WaitingForPlayers;
        private string currentPlayerName;
        private double timeLeft;
        #endregion

        #region Properties
        public enum GameStatusType { WaitingForPlayers, FullGame }
        public string HostName { get; set; }
        public string GuestName { get; set; }
        public int MaxPlayers { get; set; } = 2;
        public bool LeaveWaitingRoomVisible
        {
            get => !game.IsFull;
        }
        public GameStatusType GameStatus
        {
            get => gameStatus;
            set
            {
                if (gameStatus != value)
                {
                    gameStatus = value;
                    OnPropertyChanged(nameof(GameStatus));

                    if (IsRoomFull)
                    {
                        StartCountdown((int)countdownTime);
                    }
                }
            }
        }
        public string CurrentPlayerName
        {
            get => currentPlayerName;
            set
            {
                if (currentPlayerName != value)
                {
                    currentPlayerName = value;
                    OnPropertyChanged(nameof(CurrentPlayerName));
                }
            }
        }
        public double TimeLeft
        {
            get => timeLeft;
            set
            {
                if (timeLeft != value)
                {
                    timeLeft = value;
                    OnPropertyChanged(nameof(TimeLeft));
                    OnPropertyChanged(nameof(ProgressTimeLeft));

                    if (timeLeft <= 0 && !IsPopupClosed)
                    {
                        IsPopupClosed = true;
                        gameTimer.Stop();
                        popup.Close();
                    }
                }
            }
        }
        public double ProgressTimeLeft => TimeLeft / 5.0;
        public bool IsRoomWaiting => gameStatus == GameStatusType.WaitingForPlayers;
        public bool IsRoomFull => gameStatus == GameStatusType.FullGame;
        public bool IsRoomEmpty => game.CurrentPlayers == 0;
        public bool IsRoomNotEmpty => game.CurrentPlayers > 0;
        #endregion

        #region ICommands
        public ICommand CloseWaitingRoomCommand { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the ViewModel for the waiting room popup. It sets up the game data, the current player name,
        /// and initializes the necessary commands. The constructor also sets the initial game status and subscribes to 
        /// the event for waiting room property changes.
        /// </summary>
        /// <param name="game">The game instance containing the current game data.</param>
        /// <param name="currentPlayerName">The name of the current player.</param>
        /// <param name="popup">The popup instance representing the waiting room popup.</param>
        public WaitingRoomPopUpVM(Game game, string currentPlayerName, Popup popup)
        {
            this.fbd = new FbData();
            this.game = game;
            this.popup = popup;

            gameTimer = new GameTimer();
            HostName = game.HostName;  
            GuestName = game.GuestName ?? "Waiting...";

            CurrentPlayerName = currentPlayerName;

            CloseWaitingRoomCommand = new Command(CloseWaitingRoom);

            OnPropertyChanged(nameof(GuestName));
            UpdateGameStatus();

            game.WaitingRoomPropertiesChanged += OnWaitingRoomPropertiesChanged;
        }
        #endregion

        #region Functions
        /// <summary>
        /// Closes the waiting room popup and navigates back to the previous page. It also deletes the current game document from Firebase.
        /// </summary>
        private void CloseWaitingRoom()
        {
            popup.Close();
            Shell.Current.Navigation.PopAsync();
            fbd.DeleteDocument(Constants.collectionName, game.Id, OnComplete);
        }

        /// <summary>
        /// Handles the event when properties of the waiting room change. It updates the game status and the guest name 
        /// on the main thread, ensuring that the UI is updated with the latest information.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the game object.</param>
        /// <param name="e">Event arguments containing the event data.</param>
        private void OnWaitingRoomPropertiesChanged(object sender, EventArgs e)
        {
            Game gameObject = (Game)sender;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged(nameof(GuestName));

                UpdateGameStatus();
            });
        }

        /// <summary>
        /// Updates the game status based on the current number of players. The method checks if the game is full or if it is still waiting for players.
        /// It also updates several UI properties related to the waiting room's status.
        /// </summary>
        private void UpdateGameStatus()
        {
            GuestName = game.GuestName;
            GameStatus = game.CurrentPlayers == game.MaxPlayers
                ? GameStatusType.FullGame
                : GameStatusType.WaitingForPlayers;

            OnPropertyChanged(nameof(GuestName));
            OnPropertyChanged(nameof(IsRoomWaiting));
            OnPropertyChanged(nameof(IsRoomFull));
            OnPropertyChanged(nameof(IsRoomEmpty));
            OnPropertyChanged(nameof(IsRoomNotEmpty));
            OnPropertyChanged(nameof(LeaveWaitingRoomVisible));
        }

        /// <summary>
        /// Starts the countdown timer for the game with the specified time. The method registers a messenger to update the time left
        /// and starts the game timer if the countdown hasn't started yet.
        /// </summary>
        /// <param name="time">The time in seconds for the countdown to start.</param>
        private void StartCountdown(double time)
        {
            if (!countdownStarted)
            {
                countdownStarted = true;

                WeakReferenceMessenger.Default.Register<AppMessage<long>>(this, (r, m) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        TimeLeft = (double)(m.Value / 1000);
                    });
                });

                gameTimer.Start(time);
            }
        }

        /// <summary>
        /// A callback method for handling the completion of the delete document operation in Firebase.
        /// </summary>
        /// <param name="task">The task representing the result of the delete operation.</param>
        private void OnComplete(Task task)
        {
        }

        #endregion
    }
}