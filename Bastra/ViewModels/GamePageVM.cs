using Bastra.Models;
using Bastra.ModelsLogic;
using Bastra.Utilities;
using Bastra.Views;
using CommunityToolkit.Maui.Views;
using System.Windows.Input;
using Plugin.Maui.Audio;


namespace Bastra.ViewModels
{
    public class GamePageVM : ObservableObject
    {
        #region Fields
        private readonly FbData fbd;
        private readonly ScrollView scrlMyCards;
        private readonly Grid grdTableCards;
        private readonly Grid grdMyCards;
        private readonly GamePage gamePage;
        private readonly Game game;
        private readonly System.Timers.Timer tmrTurn;
        private Popup waitingRoomPopup;
        private double timerValue;
        private bool isWaitingRoomPopupOpened = false;
        private readonly string myName;
        private string targetTurnText = "";
        private string animatedTurnText = "";
        private string opponnentName;
        private int turnTextIndex = 0;
        private int packageCards;
        private int rounds;
        private int opponentScore;
        private int opponentCardCount;
        private int myCardCount;
        private int myScore;
        private int pickedCardsCount;

        private IAudioPlayer? _takeCardsPlayer;


        #endregion

        #region Properties
        public string AnimatedTurnText
        {
            get => animatedTurnText;
            set
            {
                if (animatedTurnText != value)
                {
                    animatedTurnText = value;
                    OnPropertyChanged();
                }
            }
        } 
        public int ScoreTarget => game.ScoreTarget;
        public bool CanCollectCards => game.CanCollectCards;
        public bool CanThrowCard => game.CanThrowCard;
        public bool IsSelectedMatch => game.IsSelectedMatch;
        public double TimerValue
        {
            get => timerValue;
            set
            {
                if (timerValue != value)
                {
                    timerValue = value;
                    OnPropertyChanged(nameof(TimerValue));
                }
            }
        }
        public string CurrentPlayerName => myName;
        public string OpponnentName
        {
            get => opponnentName;
            set
            {
                if (opponnentName != value)
                {
                    opponnentName = value;
                    OnPropertyChanged(nameof(OpponnentName));
                }
            }

        }
        public int PackageCards
        {
            get => packageCards;
            set
            {
                if (packageCards != value)
                {
                    packageCards = value;
                    OnPropertyChanged(nameof(PackageCards));
                }
            }
        }
        public int Rounds
        {
            get => rounds;
            set
            {
                if (rounds != value)
                {
                    rounds = value;
                    OnPropertyChanged(nameof(Rounds));
                }
            }
        }
        public int OpponentScore
        {
            get => opponentScore;
            set
            {
                if (opponentScore != value)
                {
                    opponentScore = value;
                    OnPropertyChanged(nameof(OpponentScore));
                }
            }
        }
        public int OpponentCardCount
        {
            get => opponentCardCount;
            set
            {
                if (opponentCardCount != value)
                {
                    opponentCardCount = value;
                    OnPropertyChanged(nameof(OpponentCardCount));
                }
            }
        }
        public int MyCardCount
        {
            get => myCardCount;
            set
            {
                if (myCardCount != value)
                {
                    myCardCount = value;
                    OnPropertyChanged(nameof(MyCardCount));
                }
            }
        }
        public int MyScore
        {
            get => myScore;
            set
            {
                if (myScore != value)
                {
                    myScore = value;
                    OnPropertyChanged(nameof(MyScore));
                }
            }
        }
        public int PickedCardsCount
        {
            get => pickedCardsCount;
            set
            {
                if (pickedCardsCount != value)
                {
                    pickedCardsCount = value;
                    OnPropertyChanged(nameof(PickedCardsCount));
                }
            }
        }
        public double ScreenWidth { get; set; }
        public double ScreenHeight { get; set; }
        #endregion

        #region ICommands
        public ICommand OpenLeaveGamePopUpCommand { get; private set; }
        public ICommand CollectCardsCommand { get; private set; }
        public ICommand SelectPlayerCardCommand { get; private set; }
        public ICommand SelectTableCardCommand { get; private set; }
        public ICommand ThrowCardCommand { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the ViewModel for the game page, setting up all necessary dependencies,
        /// event handlers, and commands for game interaction. The constructor also initializes
        /// the timer for turn updates and binds game events to UI updates.
        /// </summary>
        /// <param name="gamePage">The GamePage instance associated with this ViewModel.</param>
        /// <param name="game">The Game instance containing the current game state.</param>
        /// <param name="grdMyCards">The Grid UI element representing the player's hand of cards.</param>
        /// <param name="grdTableCards">The Grid UI element representing the table's cards.</param>
        /// <param name="scrlMyCards">The ScrollView UI element for scrolling the player's hand.</param>
        /// <param name="myName">The name of the current player.</param>
        public GamePageVM(GamePage gamePage, Game game, Grid grdMyCards, Grid grdTableCards, ScrollView scrlMyCards, string myName)
        {          
            fbd = new FbData();
            this.grdMyCards = grdMyCards;
            this.game = game;
            this.gamePage = gamePage;
            this.grdTableCards = grdTableCards;
            this.scrlMyCards = scrlMyCards;
            this.myName = fbd.DisplayName;
            PickedCardsCount = this.game.PickedCardsCount;

            tmrTurn = new System.Timers.Timer(150);
            tmrTurn.Elapsed += OnTurnTimerElapsed;

            this.game.WaitingRoomPropertiesChanged += OnWaitingRoomPropertiesChanged;
            this.game.OnTableCardsUpdated += DisplayUpdatedTableCards;
            this.game.OnGameStateChanged += UpdateGameStats;
            this.game.OnPlayerTurnChanged += PlayerTurnChanged;

            game.OnMyPropetyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(game.CanCollectCards))
                {
                    OnPropertyChanged(nameof(CanCollectCards));
                }
                else if(e.PropertyName == nameof(game.CanThrowCard))
                {
                    OnPropertyChanged(nameof(CanThrowCard));
                }
            };

            AnimateTurnText();
            SetBorderSize();

            OpenLeaveGamePopUpCommand = new Command(OpenLeaveGamePopUp);
            CollectCardsCommand = new Command(CollectCards);
            SelectPlayerCardCommand = new Command<SelectCardEventArgs>(SelectPlayerCard);
            SelectTableCardCommand = new Command<SelectTableCardEventArgs>(SelectTableCard);
            ThrowCardCommand = new Command<SelectTableCardEventArgs>(ThrowCard);

            TakePackageCard(false);
        }
        #endregion

        #region Functions
        private void OnTurnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (turnTextIndex < targetTurnText.Length)
            {
                AnimatedTurnText += targetTurnText[turnTextIndex];
                turnTextIndex++;
            }
            else
                tmrTurn.Stop();

        }
        /// <summary>
        /// Animates the turn text on the game page. It updates the text that indicates whether it is
        /// the player's turn to play, or if they need to wait. If the game is over or not full, the text is cleared.
        /// The text is animated character by character, and the timer for animation is started.
        /// </summary>
        private void AnimateTurnText()
        {
            if (game.WinnerName.Length > 0 || !game.IsFull)
                targetTurnText = string.Empty;
            else
                targetTurnText = game.MyPlayerType == game.PlayingNow ? "Play" : "Wait";

            turnTextIndex = 0;
            AnimatedTurnText = string.Empty;
            tmrTurn.Start();
        }
        /// <summary>
        /// Handles the event when the player's turn changes. It updates the turn text and triggers the animation
        /// of the turn text based on the new turn state. It also notifies the UI of the change in the animated turn text.
        /// </summary>
        /// <param name="sender">The sender of the event (typically the game object).</param>
        /// <param name="e">Event arguments containing the event data.</param>
        private void PlayerTurnChanged(object? sender, EventArgs e)
        {
            targetTurnText = game.GetPlayerTurnChangedText();
            AnimateTurnText();
            OnPropertyChanged(nameof(AnimatedTurnText));
        }
        /// <summary>
        /// Handles the logic for throwing a card from the player's hand to the table.
        /// The method updates the game state, disables card actions (throw and collect), 
        /// and updates the UI by adding the thrown card to the table grid and updating relevant properties.
        /// It also triggers the next player's turn and updates game stats.
        /// </summary>
        /// <param name="args">Event arguments containing the selected table card.</param>
        private void ThrowCard(SelectTableCardEventArgs args)
        {
            Card tableCard = game.ThrowCard();
            game.CanThrowCard = false;
            game.CanCollectCards = false;
            if (!tableCard.IsEmpty)
            {
                game.NextPlay();
                SelectTableCardEventArgs stcea = new() { SelectedCard = tableCard };

                tableCard.CommandParameter = stcea;
                tableCard.Command = SelectTableCardCommand;

                tableCard.Margin = new Thickness(50 + 30 * game.GetTableCardsCount, 0, 0, 0);
                grdTableCards.Add(tableCard);

                if (game.GetMyCards.Count == 0)
                    TakePackageCard(true);

                UpdateGameStats();
                OnPropertyChanged(nameof(grdTableCards));
                OnPropertyChanged(nameof(scrlMyCards));
                OnPropertyChanged(nameof(grdMyCards));
                OnPropertyChanged(nameof(CanCollectCards));
                OnPropertyChanged(nameof(CanThrowCard));
                OnPropertyChanged(nameof(PickedCardsCount));
            }
        }
        /// <summary>
        /// Handles the logic for collecting the selected cards from the table and updating the game state.
        /// The method triggers the next player's turn, updates the player's card count, and disables 
        /// actions for collecting and throwing cards. It also checks if the player needs to draw more cards 
        /// and updates the UI with the latest game state.
        /// </summary>
        private void CollectCards()
        {
            game.NextPlay();
            game.CollectCards();
            game.CanCollectCards = false;
            game.CanThrowCard = false;
            PickedCardsCount = game.PickedCardsCount;

            if (game.GetMyCards.Count == 0)
                TakePackageCard(true);

            UpdateGameStats();
            OnPropertyChanged(nameof(grdTableCards));
            OnPropertyChanged(nameof(scrlMyCards));
            OnPropertyChanged(nameof(grdMyCards));
            OnPropertyChanged(nameof(CanThrowCard));
            OnPropertyChanged(nameof(CanCollectCards));
            OnPropertyChanged(nameof(PickedCardsCount));
        }
        /// <summary>
        /// Handles the logic for selecting a card from the player's hand. The method ensures that the player can
        /// only select a card when it is their turn. It waits for the animation to complete and then refreshes the
        /// relevant UI bindings for the game state, including whether the player can collect cards, throw cards,
        /// or other actions based on the current turn.
        /// </summary>
        /// <param name="args">Event arguments containing the selected card.</param>
        private async void SelectPlayerCard(SelectCardEventArgs args)
        {
            if (args.SelectedCard != null && game.MyPlayerType == game.PlayingNow)
            {
                await game.SelectCard(args.SelectedCard);
                // After awaiting the full animation, refresh the UI binding.
                OnPropertyChanged(nameof(CanCollectCards));
                OnPropertyChanged(nameof(IsSelectedMatch));
                OnPropertyChanged(nameof(CanThrowCard));
                OnPropertyChanged(nameof(PickedCardsCount));
                // Also update any UI elements showing card collections.
                OnPropertyChanged(nameof(grdTableCards));
                OnPropertyChanged(nameof(scrlMyCards));
                OnPropertyChanged(nameof(grdMyCards));
            }
        }
        /// <summary>
        /// Handles the logic for selecting a card from the table. The method ensures that the player can 
        /// only select a card when it is their turn. It updates the relevant game state and refreshes the
        /// UI bindings after the selection, including properties like whether the cards can be collected or thrown.
        /// </summary>
        /// <param name="args">Event arguments containing the selected card from the table.</param>
        private void SelectTableCard(SelectTableCardEventArgs args)
        {
            if (args.SelectedCard != null)
            {
                if (game.MyPlayerType == game.PlayingNow)
                    game.SelectTableCard(args.SelectedCard);
                OnPropertyChanged(nameof(IsSelectedMatch));
                OnPropertyChanged(nameof(grdTableCards));
                OnPropertyChanged(nameof(scrlMyCards));
                OnPropertyChanged(nameof(grdMyCards));
                OnPropertyChanged(nameof(CanCollectCards));
                OnPropertyChanged(nameof(PickedCardsCount));

            }
        }

        /// <summary>
        /// Handles the logic for taking a card from the package and adding it to the player's hand or the table.
        /// The method also ensures that the cards are displayed correctly in the UI, and it updates the relevant bindings.
        /// If it's a new round, the game is reset, and table cards are fetched from Firebase.
        /// </summary>
        /// <param name="newRound">Indicates whether the card draw is for a new round.</param>
        private async void TakePackageCard(bool newRound)
        { 
            CardsSet myCards = game.GetMyCards;
            if (!newRound)
            {
                if (myCards != null)
                {
                    await PlayTakeCardsSoundAsync();
                    foreach (Card card in myCards.CardSetToList)
                    {
                        grdMyCards.Add(card);
                        SelectCardEventArgs scea = new() { SelectedCard = card };
                        card.Command = SelectPlayerCardCommand;
                        card.CommandParameter = scea;
                    }
                    if (!game.IsFull)
                    {
                        CardsSet tableCards = game.GetTableCards;
                        foreach (Card card in tableCards.CardSetToList)
                        {
                            grdTableCards.Add(card);
                            SelectTableCardEventArgs stcea = new() { SelectedCard = card };
                            card.CommandParameter = stcea;
                            card.Command = SelectTableCardCommand;
                        }
                    }
                    scrlMyCards.ScrollToAsync(grdTableCards, ScrollToPosition.Start, true);
                    OnPropertyChanged(nameof(PickedCardsCount));
                }

            }
            else
            {
                await PlayTakeCardsSoundAsync();
                game.Reset(newRound);
                game.FetchTableCardsFromFirebase();
                foreach (Card card in myCards.CardSetToList)
                {
                    grdMyCards.Add(card);
                    SelectCardEventArgs scea = new() { SelectedCard = card };
                    card.Command = SelectPlayerCardCommand;
                    card.CommandParameter = scea;
                }
                scrlMyCards.ScrollToAsync(grdTableCards, ScrollToPosition.Start, true);
                OnPropertyChanged(nameof(PickedCardsCount));
            }
        }
        /// <summary>
        /// Updates the table cards displayed in the UI by clearing the existing cards and 
        /// adding the updated cards from the game. This method also reassigns the selection
        /// command to each table card and triggers the necessary UI updates for the table cards.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the game object.</param>
        /// <param name="e">Event arguments containing the event data.</param>
        private void DisplayUpdatedTableCards(object? sender, EventArgs e)
        {
            grdTableCards.Clear(); // Clear current table cards

            CardsSet tableCards = game.GetTableCards; // Get updated table cards
            foreach (Card card in tableCards.CardSetToList)
            {
                tableCards.FixMargin(false);
                grdTableCards.Add(card);

                SelectTableCardEventArgs selectTableCardArgs = new SelectTableCardEventArgs { SelectedCard = card };
                card.CommandParameter = selectTableCardArgs;
                card.Command = SelectTableCardCommand; // Re-assign the command
            }

            // Trigger UI updates
            scrlMyCards.ScrollToAsync(grdTableCards, ScrollToPosition.Start, true);
            OnPropertyChanged(nameof(grdTableCards));
        }
        /// <summary>
        /// Updates the game statistics by calling the overloaded version of the method.
        /// This method is used to trigger an update without additional parameters.
        /// </summary>
        private void UpdateGameStats()
        {
            UpdateGameStats(null, EventArgs.Empty);
        }
        /// <summary>
        /// Updates the game statistics based on the current game state, including the player's score,
        /// card count, opponent's score and card count, the number of remaining package cards, and the current round.
        /// The method differentiates the player's data based on whether they are the host or guest.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the game object.</param>
        /// <param name="e">Event arguments containing the event data.</param>
        private async void UpdateGameStats(object? sender, EventArgs e)
        {
            if (this.game != null)
            {
                if (game.MyPlayerType == GameModel.PlayerType.Host)
                {
                    MyScore = game.HostScore;
                    MyCardCount = game.HostCardsCtr;

                    OpponentScore = game.GuestScore;
                    OpponentCardCount = game.GuestCardsCtr;
                    OpponnentName = game.GuestName;
                }
                else
                {
                    MyScore = game.GuestScore;
                    MyCardCount = game.GuestCardsCtr;

                    OpponentScore = game.HostScore;
                    OpponentCardCount = game.HostCardsCtr;
                    OpponnentName = game.HostName;
                }
                PackageCards = game.PackageCardsAmount;
                Rounds = game.GetGameRounds;                             
            }
        }
        
        /// <summary>
        /// Opens the "Leave Game" popup, allowing the player to exit the game.
        /// This method delegates the functionality to the <see cref="game.OpenLeaveGamePopUp"/> method.
        /// </summary>
        private void OpenLeaveGamePopUp()
        {
            game.OpenLeaveGamePopUp();
        }

        /// <summary>
        /// Displays the "Win" popup when the game has ended and a winner has been determined.
        /// This method delegates the functionality to the <see cref="game.ShowWinPopUp"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the game object.</param>
        /// <param name="e">Event arguments containing the event data.</param>
        //private void ShowWinPopup(object sender, EventArgs e)
        //{
        //    game.ShowWinPopUp();
        //}

        /// <summary>
        /// Handles the event when the game has ended, triggering the display of the end game popup.
        /// This method delegates the functionality to the <see cref="game.OnGameEndedPopUp"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the game object.</param>
        /// <param name="e">Event arguments containing the event data.</param>
        //private async void OnGameEnded(object sender, EventArgs e)
        //{
        //    game.OnGameEndedPopUp();
        //}

        /// <summary>
        /// Handles the event when properties of the waiting room change. It displays a waiting room popup 
        /// if it's not already open. The popup is displayed on the main thread using <see cref="MainThread.BeginInvokeOnMainThread"/>.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the game object.</param>
        /// <param name="e">Event arguments containing the event data.</param>
        private void OnWaitingRoomPropertiesChanged(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Game gameObject = (Game)sender;

                if (!isWaitingRoomPopupOpened)
                {
                    waitingRoomPopup = new WaitingRoomPopUp(gameObject, myName);
                    Shell.Current.ShowPopup(waitingRoomPopup);
                    isWaitingRoomPopupOpened = true;
                }
            });
        }

        /// <summary>
        /// Adds a snapshot listener to the game document in Firebase, enabling real-time updates
        /// whenever the game data changes. This method delegates the functionality to the <see cref="game.AddSnapshotListener"/> method.
        /// </summary>
        public void AddSnapshotListener()
        {
            game.AddSnapshotListener();
        }

        /// <summary>
        /// Removes the snapshot listener from the game document in Firebase. This stops real-time updates
        /// from being triggered for the game document. Additionally, it unsubscribes from the table cards update event.
        /// </summary>
        public void RemoveSnapshotListener()
        {
            game.RemoveSnapshotListener();
            game.OnTableCardsUpdated -= DisplayUpdatedTableCards;
        }

        /// <summary>
        /// Deletes the game document from Firebase if the game is full. The method also updates the number of games played
        /// and the date of the last game played in local preferences.
        /// </summary>
        public void DeleteDocument()
        {
            if (game.IsFull)
            {
                int gamesPlayed = Preferences.Get(Constants.GamesPlayedKey, 0);
                Preferences.Set(Constants.GamesPlayedKey, gamesPlayed + 1);

                DateTime now = DateTime.Now;
                Preferences.Set(Constants.LastGamePlayedDateKey, now.ToString("dd/MM/yyyy HH:mm"));
                game.DeleteDocument();
            }
        }

        /// <summary>
        /// Sets the screen border size based on the device's display width and height. The method calculates
        /// the screen dimensions in device-independent units by dividing the width and height by the screen density.
        /// </summary>
        private void SetBorderSize()
        {
            ScreenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            ScreenHeight = -50 + DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        }
        #endregion

        #region SoundsPlayer
        private async Task PlayTakeCardsSoundAsync()
        {
            if (_takeCardsPlayer == null)
            {
                Stream stream = await FileSystem.OpenAppPackageFileAsync("TakeCardsSound.mp3");
                _takeCardsPlayer = AudioManager.Current.CreatePlayer(stream);
            }

            if (_takeCardsPlayer.IsPlaying)
                _takeCardsPlayer.Seek(0);

            _takeCardsPlayer.Play();
        }
        #endregion
    }
}