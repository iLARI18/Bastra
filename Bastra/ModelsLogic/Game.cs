using Bastra.Models;
using Bastra.Utilities;
using Bastra.Views;
using CommunityToolkit.Maui.Views;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;

namespace Bastra.ModelsLogic
{
    public class Game : GameModel
    {
        #region Fields
        private readonly FbData fbd;
        private IListenerRegistration ilr;
        private bool tableInitialized = false;
        private bool isGameEndedPopupOpened = false;
        private int playerSelectedCardSum = 0;
        private int tableSelectedCardsSum = 0;
        private int tableSelectedCardsCtr = 0;
        private int tableCardsCtr = 4;
        private Card tableSingleCard;
        private Card playerSingleCard;
        private readonly string myName;
        private Popup leaveGamePopup;
        private bool isJackAnimationComplete = true;
        private bool jackDownActive = false;
        private bool isSelectedMatch;


        #endregion

        #region Properties
        [Ignored]
        public int PackageCardsAmount => package.Count;
        [Ignored]
        public bool IsSelectedMatch => isSelectedMatch;
        [Ignored]
        public bool CanCollectCards { get; set; } = false;
        [Ignored]
        public bool CanThrowCard { get; set; } = false;
        [Ignored]
        public int GetGameRounds => GameRounds;
        [Ignored]
        public int GetTableCardsCount => tableCards.Count;
        [Ignored]
        public CardsSet GetMyCards => myCards;
        [Ignored]
        public CardsSet GetTableCards => tableCards;

        public List<string> PackageStrings
        {
            get => package.ToStringArray;
            set => package.FromStringArray(value);
        }
        public List<string> TableCardsStrings
        {
            get => tableCards.ToStringArray;
            set => tableCards.FromStringArray(value);
        }
        #endregion

        #region Events
        public event EventHandler WaitingRoomPropertiesChanged;
        public event EventHandler? OnTableCardsUpdated;
        public event EventHandler GameCreated;
        public event EventHandler OnGameStateChanged;
        public event EventHandler? OnPlayerTurnChanged;
        public event EventHandler<PropertyEventArgs> OnMyPropetyChanged;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// Sets up Firebase data access, player's name, game manager instance,
        /// player card sets, and table cards if the game is not full. Also fetches
        /// table cards from Firebase and resets the game state.
        /// </summary>
        public Game() : base()
        {
            fbd = new FbData();
            myName = fbd.DisplayName;
            GameManager.Instance.CurrentGame = this;

            myCards = new CardsSet(false)
            {
                SingleSelect = true,
            };
            collectedCards = new CardsSet(false);

            if (!IsFull)
            {
                Created = DateTime.Now;
                HostName = fbd.DisplayName;

                tableCards = new CardsSet(false)
                {
                    SingleSelect = false
                };
            }
            FetchTableCardsFromFirebase();
            Reset(false);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Handles the logic for throwing the currently selected player card to the table.
        /// Updates the table and Firebase, resets selection states and notifies listeners of the turn change.
        /// </summary>
        /// <returns>
        /// The card that was thrown, or an empty card if no valid card was selected.
        /// </returns>
        public Card ThrowCard()
        {
            Card card = myCards.ThrowCard(playerSingleCard);
            ActionText = GetCardActionText(card);
            fbd.UpdateField(Constants.collectionName, Id, "ActionText", ActionText);
            if (!card.IsEmpty)
            {
                tableCards.Add(card);
                FetchTableCardsFromFirebase();
                UpdateFirebase();
                playerSelectedCardSum = 0;
                tableSelectedCardsSum = 0;
                isSelectedMatch = false;
                playerSingleCard.IsSelected = false;
            }
            playerSingleCard = null;

            tableCards.FixMargin(false);
            myCards.FixMargin(true);
            tableCards.MakeVisible();
            OnPlayerTurnChanged?.Invoke(this, EventArgs.Empty);

            return card;
        }
        private string GetCardActionText(Card card)
        {
            string cardName;

            switch (card.Value)
            {
                case 1:
                    cardName = "Ace";
                    break;
                case 11:
                    cardName = "Jack";
                    break;
                case 12:
                    cardName = "Queen";
                    break;
                case 13:
                    cardName = "King";
                    break;
                default:
                    cardName = card.Value.ToString();
                    break;
            }

            return $"{myName} threw {cardName} {card.Shape}";
        }

        /// Collects the player's currently selected card from their hand,
        /// updates the score accordingly, and adds the card to the collected pile.
        /// Also resets selection-related fields and counters.
        /// </summary>
        /// <returns>The card that was collected from the player's hand.</returns>
        public Card CollectPlayerCard()
        {
            Card card = myCards.RemoveSelectedCard(playerSingleCard);

            UpdateScore(card, false);
            collectedCards.Add(card);

            PickedCardsCount++;
            playerSelectedCardSum = 0;
            playerSingleCard = null;
            tableSelectedCardsSum = 0;
            isSelectedMatch = false;

            return card;
        }
        /// <summary>
        /// Collects the selected cards from the table and adds them to the player's collected pile.
        /// Calculates and applies score updates, including a Basra bonus if applicable.
        /// Also updates Firebase and resets relevant selection counters.
        /// </summary>
        /// <returns>The updated table card set after removal.</returns>
        public async Task<CardsSet> CollectTableCards()
        {
            int tableCardsCount = tableCards.Count;
            List<Card> removedCards = tableCards.RemoveSelectedCards();

            bool isBastra = CheckForBasra(playerSingleCard, removedCards, tableCardsCount);
            bool bonusApplied = false;

            if (isBastra)
            {
                fbd.UpdateField(Constants.collectionName, Id, "ActionText", "Busstra");
            }

            foreach (Card card in removedCards)
            {
                if (!bonusApplied && isBastra)
                {
                    UpdateScore(card, true);
                    bonusApplied = true;
                }
                else
                {
                    UpdateScore(card, false);
                }

                collectedCards.Add(card);
                tableCardsCtr--;
            }

            tableSelectedCardsSum = 0;
            tableSelectedCardsCtr = 0;
           
            FetchTableCardsFromFirebase();
            UpdateFirebase();

            return tableCards;
        }


        /// <summary>
        /// Checks if a Basra occurred, which happens when a player clears the table
        /// (i.e., captures all cards except with a Jack, which doesn't count).
        /// </summary>
        /// <param name="playedCard">The card the player used to capture.</param>
        /// <param name="capturedCards">The list of cards captured from the table.</param>
        /// <param name="tableCardsCount">The number of cards that were on the table before capture.</param>
        /// <returns>True if it's a Basra; otherwise, false.</returns>
        private bool CheckForBasra(Card playedCard, List<Card> capturedCards, int tableCardsCount)
        {
            return capturedCards.Count == tableCardsCount && playedCard.Value != 11;
        }


        /// <summary>
        /// Advances the game to the next player's turn.
        /// Disables Jack action state, updates the current turn player,
        /// triggers the <see cref="OnPlayerTurnChanged"/> event,
        /// and updates the change in Firebase.
        /// </summary>
        public void NextPlay()
        {
            jackDownActive = false;
            PlayingNow = (PlayingNow == PlayerType.Host) ? PlayerType.Guest : PlayerType.Host;
            OnPlayerTurnChanged?.Invoke(this, EventArgs.Empty);
            fbd.UpdateField(Constants.collectionName, Id, "PlayingNow", PlayingNow);
        }

        /// <summary>
        /// Handles the selection or deselection of a card from the player's hand,
        /// and updates game state accordingly. Includes special logic for Jack cards
        /// (value 11), triggering animations and affecting the ability to collect or throw cards.
        /// Also raises UI property change events for UI reactivity.
        /// </summary>
        /// <param name="card">The card selected or deselected by the player.</param>
        public async Task SelectCard(Card card)
        {
            myCards.SelectCard(card);
            if (card.IsSelected)
                playerSingleCard = card;
            else
                playerSingleCard = null;

            OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanCollectCards)));
            OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanThrowCard)));

            if (card != null)
            {
                if (card.Value == 11)
                {
                    isJackAnimationComplete = false;
                    if (card.IsSelected)
                    {
                        tableSingleCard = null;
                        jackDownActive = false;
                        CanCollectCards = false;
                        CanThrowCard = false;
                        OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanCollectCards)));
                        OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanThrowCard)));
                        await tableCards.Animation(true,myCards);
                        CanCollectCards = tableCards.Count > 0 ? true : false;
                        CanThrowCard = true;
                        OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanCollectCards)));
                        OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanThrowCard)));

                        tableSelectedCardsCtr = tableCards.GetSelectedCardsCtr();
                        tableSelectedCardsSum = tableCards.GetSelectedCardsSum();
                    }
                    else
                    {
                        jackDownActive = true;
                        playerSingleCard = null;
                        tableSingleCard = null;
                        playerSelectedCardSum = 0;

                        CanCollectCards = false;
                        CanThrowCard = false;
                        OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanCollectCards)));
                        OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanThrowCard)));
                        await tableCards.Animation(false, myCards);
                        tableSelectedCardsCtr = 0;
                        tableSelectedCardsSum = 0;
                    }
                    isJackAnimationComplete = true;
                }
                else
                {
                    if (card.IsSelected)
                    {
                        CanThrowCard = true;
                        playerSingleCard = card;
                        playerSelectedCardSum = card.Value;
                        CanCollectCards = CalculateCanCollectCards();
                        OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanThrowCard)));
                        OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanCollectCards)));

                        
                        tableCards.MakeAllEnable();

                        tableSelectedCardsCtr = tableCards.GetSelectedCardsCtr();
                        tableSelectedCardsSum = tableCards.GetSelectedCardsSum();
                    }
                    else
                    {
                        playerSingleCard = null;
                        tableSingleCard = null;
                        playerSelectedCardSum = 0;
                        CanCollectCards = false;
                        CanThrowCard = false;

                        OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanCollectCards)));
                        OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanThrowCard)));
                        await tableCards.Animation(false, myCards);
                        tableSelectedCardsCtr = 0;
                        tableSelectedCardsSum = 0;
                    }
                }
            }
            OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanCollectCards)));
            OnMyPropetyChanged?.Invoke(this, new PropertyEventArgs(nameof(CanThrowCard)));
        }

        /// <summary>
        /// Handles the selection or deselection of a card from the table.
        /// Updates counters and selection state, tracks a single selected card (if only one is selected),
        /// and recalculates whether the player can collect the selected cards.
        /// </summary>
        /// <param name="card">The card on the table that was selected or deselected.</param>
        public void SelectTableCard(Card card)
        {
            tableCards.SelectCard(card);
            if (card.IsSelected)
            {
                tableSelectedCardsSum += card.Value;
                tableSelectedCardsCtr++;

                if (tableSelectedCardsCtr == 1)
                    tableSingleCard = card;
                else
                    tableSingleCard = null;
            }
            else
            {
                tableSelectedCardsSum -= card.Value;
                tableSelectedCardsCtr--;

                if (tableSelectedCardsCtr == 1)
                {
                    List<Card> selected = tableCards.GetSelectedCards();
                    if (selected != null && selected.Count > 0)
                        tableSingleCard = selected.First();
                }

                else
                    tableSingleCard = null;
            }
            CanCollectCards = CalculateCanCollectCards();
        }

        /// <summary>
        /// Determines whether the current player is allowed to collect cards from the table
        /// based on the selected cards and game rules, including special handling for Jack, Queen, and King.
        /// </summary>
        /// <returns>
        /// True if the collection is valid based on game logic; otherwise, false.
        /// </returns>
        public bool CalculateCanCollectCards()
        {
            bool result = false;
            bool finalCheckNeeded = true;

            // If a jack was taken down, immediately disable collection.
            if (jackDownActive)
            {
                jackDownActive = false;
                result = false;
            }
            // Also, if a jack animation is still in progress, disable collection.
            else if (!isJackAnimationComplete)
            {
                isJackAnimationComplete = true;
                result = false;
            }
            else if (tableSelectedCardsCtr == 0)
                result = false;
            // If a jack is selected (and animation complete), allow collection.
            else if (playerSingleCard != null && playerSingleCard.Value == 11 && tableCards.Count>0)
                result = true;

           

            else if (playerSingleCard != null && playerSingleCard.Value > 0)
            {
                // For Queen or King, use existing logic.
                if ((playerSingleCard.Value == 13 )|| (playerSingleCard.Value == 12))
                {
                    if (playerSingleCard.IsSelected)                   
                        result = tableSingleCard != null && playerSingleCard.Value == tableSingleCard.Value;                    
                    finalCheckNeeded = false;
                }

                if (tableSingleCard != null)
                {
                    result = tableSingleCard.Value == playerSingleCard.Value;
                    finalCheckNeeded = false;
                }

                bool biggerCardFound = false;
                if (finalCheckNeeded)
                {
                    List<Card> selectedTableCards = tableCards.GetSelectedCards();
                    foreach (Card tableCard in selectedTableCards)
                    {
                        if (tableCard.Value > playerSingleCard.Value)
                        {
                            biggerCardFound = true;
                            break;
                        }
                    }
                    if (biggerCardFound)
                    {
                        result = false;
                        finalCheckNeeded = false;
                    }
                }

                // Finally, perform the sum-based check.
                if (finalCheckNeeded)
                {
                    result = tableSelectedCardsSum != 0 &&
                             playerSelectedCardSum != 0 &&
                             tableSelectedCardsSum % playerSelectedCardSum == 0 &&
                             playerSelectedCardSum != 11 &&
                             tableSelectedCardsCtr > 0;
                }
            }
            CanCollectCards = result;
            return CanCollectCards;
        }

        /// <summary>
        /// Updates the score of the current player based on the card collected and whether it triggered a Basra.
        /// Also updates the count of collected cards and synchronizes the data with Firebase.
        /// Special cards grant bonus points:
        /// - ♦10 = 3 points
        /// - ♣2 = 2 points
        /// - Aces and Jacks = 1 point
        /// A Basra adds an additional 10 points.
        /// </summary>
        /// <param name="card">The card that was collected.</param>
        /// <param name="isBastra">True if the move was a Basra (cleared the table); otherwise, false.</param>
        public void UpdateScore(Card card, bool isBastra)
        {
            int score = 0;

            if (MyPlayerType == PlayerType.Host)
            {
                HostCardsCtr++;
                HostRoundCardsCtr++;
                fbd.UpdateFields(Constants.collectionName, Id, new Dictionary<string, object>
                {
                    { "HostCardsCtr", HostCardsCtr },
                    { "HostRoundCardsCtr", HostRoundCardsCtr }
                });
            }
            else
            {
                GuestCardsCtr++;
                GuestRoundCardsCtr++;
                fbd.UpdateFields(Constants.collectionName, Id, new Dictionary<string, object>
                {
                    { "GuestCardsCtr", GuestCardsCtr },
                    { "GuestRoundCardsCtr", GuestRoundCardsCtr }
                });
            }

            if (card.Shape == CardModel.Shapes.Diamond && card.Value == 10)
                score = 3;
            else if (card.Shape == CardModel.Shapes.Club && card.Value == 2)
                score = 2;
            else if (card.Value == 1 || card.Value == 11)
                score = 1;

            if (isBastra)
                score += 10;

            if (MyPlayerType == PlayerType.Host)
                HostScore += score;
            else
                GuestScore += score;

            Dictionary<string, object> Score = new Dictionary<string, object>
            {
                { "HostScore", HostScore },
                { "GuestScore", GuestScore }
            };
            fbd.UpdateFields(Constants.collectionName, Id, Score);

            CheckWinner();
        }

        /// <summary>
        /// Listener callback triggered when the game document in Firebase changes.
        /// Updates local game state based on the new snapshot, including player scores,
        /// game status, and card data. Also triggers UI update events and win-check logic.
        /// </summary>
        /// <param name="snapshot">The snapshot of the game document from Firebase.</param>
        /// <param name="error">An exception, if one occurred during snapshot retrieval.</param>
        private void OnChange(IDocumentSnapshot snapshot, Exception error)
        {
            if (snapshot != null)
            {
                Game game = snapshot.ToObject<Game>();
                if (game != null)
                {
                    CurrentPlayers = game.CurrentPlayers;
                    HostName = game.HostName;
                    GuestName = game.GuestName;
                    IsFull = game.IsFull;
                    WinnerName = game.WinnerName;
                    if (MyPlayerType == PlayerType.Host)
                    {
                        GuestScore = game.GuestScore;
                        GuestCardsCtr = game.GuestCardsCtr;
                    }
                    else
                    {
                        HostScore = game.HostScore;
                        HostCardsCtr = game.HostCardsCtr;
                    }
                    if (GameRounds < game.GameRounds)
                    {
                        GuestRoundCardsCtr = game.GuestRoundCardsCtr;
                        HostRoundCardsCtr = game.HostRoundCardsCtr;
                    }
                    if (ActionText != game.ActionText && !string.IsNullOrEmpty(game.ActionText))
                    {
                        ActionText = game.ActionText;
                        PlayTextToSpeech();
                    }
                    GameRounds = game.GameRounds;
                    package = game.package;
                    PlayingNow = game.PlayingNow;
                    PackageStrings = game.PackageStrings;
                    TableCardsStrings = game.TableCardsStrings;

                    OnTableCardsUpdated?.Invoke(this, EventArgs.Empty);
                    OnGameStateChanged?.Invoke(this, EventArgs.Empty);
                    OnPlayerTurnChanged?.Invoke(this, EventArgs.Empty);
                    WaitingRoomPropertiesChanged?.Invoke(this, EventArgs.Empty);
                    if (CheckWinner())
                        ShowWinPopUp();

                }
                else
                    OnGameEndedPopUp();
            }
        }
        /// <summary>
        /// Collects both the cards from the table and the player's selected card.
        /// Updates the game state, triggers the turn change event, and syncs the data with Firebase.
        /// </summary>
        public void CollectCards()
        {
            CollectTableCards();
            CollectPlayerCard();

            OnPlayerTurnChanged?.Invoke(this, EventArgs.Empty);

            FetchTableCardsFromFirebase();
            UpdateFirebase();
        }

        /// <summary>
        /// Updates the game data in Firebase, including the package strings and table cards strings.
        /// This method prepares a dictionary with the relevant data and uses Firebase to save it.
        /// </summary>
        public void UpdateFirebase()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "PackageStrings", PackageStrings },
                { "TableCardsStrings", TableCardsStrings }
            };
            fbd.UpdateFields(Constants.collectionName, Id, data);
        }
      
        /// <summary>
        /// Fetches the table cards from Firebase based on the current game ID.
        /// The method retrieves the document with the matching ID and updates the local 
        /// <see cref="TableCardsStrings"/> property with the data from Firebase.
        /// </summary>
        public void FetchTableCardsFromFirebase()
        {
            if (Id != null)
            {
                fbd.GetDocumentsWhereEqualTo(Constants.collectionName, "Id", Id, snapshot =>
                {
                    if (snapshot.IsEmpty)
                    {
                        foreach (IDocumentSnapshot document in snapshot.Documents)
                            if (document != null)
                            {
                                Dictionary<string, object>? data = document.ToObject<Dictionary<string, object>>();
                                if (data != null && data.ContainsKey("TableCardsStrings"))
                                {
                                    List<string> tableCardsStrings = (List<string>)data["TableCardsStrings"];
                                    TableCardsStrings = tableCardsStrings.ToList();
                                }
                            }
                    }
                });
            }
        }
        /// <summary>
        /// Resets the game state for either a new round or the initial setup of the table and player hands.
        /// If the package has cards, it draws cards for the table and the player's hand, ensuring that 
        /// 4 cards are added to the table and 4 to the player's hand. Updates Firebase if it's a new round.
        /// </summary>
        /// <param name="newRound">Indicates whether the reset is for a new round of the game.</param>
        public void Reset(bool newRound)
        {
            if (package.Count > 0)
            {
                if (!tableInitialized)
                {
                    List<Card> cards = new List<Card>();
                    bool finish = false;
                    int addedCards = 0;
                    while (!finish)
                    {
                        CheckForEmptyPackage();
                        Card card = package.TakeCard();
                        if (card != null && card.Value != 11)
                        {
                            tableCards.Add(card);
                            addedCards++;
                        }
                        else if (card != null)
                            package.Add(card);

                        if (addedCards == 4)
                            finish = true;
                    }
                    tableInitialized = true;
                }

                int drawnCards = 0;
                while (drawnCards < 4)
                {
                    CheckForEmptyPackage();
                    Card card = package.TakeCard();

                    if (card != null)
                        if (package.CanAddToMyCards(card))
                        {
                            myCards.Add(card);
                            drawnCards++;
                        }
                        else
                            package.Add(card);
                    
                }
                myCards.FixMargin(true);
                tableCards.FixMargin(false);
            }
            else
                CheckForEmptyPackage();

            if (newRound)
                UpdateFirebase();
        }
        /// <summary>
        /// Checks if the card package is empty. If it is, the game progresses to the next round:
        /// - Increments the round counter,
        /// - Clears and refills the package,
        /// - Resets the game state for the next round.
        /// </summary>
        public void CheckForEmptyPackage()
        {
            if (package.Count == 0)
            {
                HandleRoundBonusPoints();
                GameRounds++;
                fbd.UpdateField(Constants.collectionName, Id, "GameRounds", GameRounds);
                package.Clear();
                package.FillPakage();
                Reset(true);
            }
        }

        private void HandleRoundBonusPoints()
        {
            // Add 3 points to the player who collected more cards in the last round
            if (HostRoundCardsCtr != GuestRoundCardsCtr)
            {
                bool hostWon = HostRoundCardsCtr > GuestRoundCardsCtr;

                if (hostWon)
                    HostScore += 7;
                else
                    GuestScore += 7;

                fbd.UpdateFields(Constants.collectionName, Id, new Dictionary<string, object>
                {
                    { "HostScore", HostScore },
                    { "GuestScore", GuestScore }
                });
            }

            // Reset round counters for next round
            HostRoundCardsCtr = 0;
            GuestRoundCardsCtr = 0;
            fbd.UpdateFields(Constants.collectionName, Id, new Dictionary<string, object>
            {
                { "HostRoundCardsCtr", HostRoundCardsCtr },
                { "GuestRoundCardsCtr", GuestRoundCardsCtr }
            });
        }

        /// <summary>
        /// Checks if there is a winner based on the target score.
        /// If a winner is found, updates the winner's name and triggers the winner check.
        /// </summary>
        /// <returns>
        /// True if a winner has been found, otherwise false.
        /// </returns>
        public bool CheckWinner()
        {
            bool returnType = false;
            if (HostScore >= ScoreTarget || GuestScore >= ScoreTarget)
            {
                returnType = true;
                WinnerName = HostScore >= ScoreTarget ? HostName : GuestName;
                fbd.UpdateField(Constants.collectionName, Id, "WinnerName", WinnerName);
            }
            return returnType;
        }

        /// <summary>
        /// Adds a snapshot listener to the game document in Firebase. This allows for real-time updates
        /// whenever the document changes. The listener will invoke the <see cref="OnChange"/> method
        /// when the document is updated.
        /// </summary>
        public void AddSnapshotListener()
        {
            ilr = fbd.AddSnapshotListener(Constants.collectionName, Id, OnChange);
        }

        /// <summary>
        /// Removes the snapshot listener for the game document in Firebase. This stops real-time updates
        /// from being triggered for this document.
        /// </summary>
        public void RemoveSnapshotListener()
        {
            ilr?.Remove();
        }

        /// <summary>
        /// Sets the target score for the game. This is the score required to win.
        /// </summary>
        /// <param name="gameScore">The target score to set for the game.</param>
        public void SetScore(int gameScore)
        {
            ScoreTarget = gameScore;
        }

        /// <summary>
        /// Deletes the game document from Firebase.
        /// </summary>
        public void DeleteDocument()
        {
            fbd.DeleteDocument(Constants.collectionName, Id, OnDelete);
        }

        /// <summary>
        /// Callback method triggered after a document is deleted in Firebase.
        /// </summary>
        /// <param name="task">The task representing the result of the delete operation.</param>
        private void OnDelete(Task task)
        {
            // Handle deletion completion (if needed)
        }

        /// <summary>
        /// Sets the game document in Firebase. If the document ID is not already set, a new ID will be generated.
        /// The document is saved under the specified collection in Firebase.
        /// </summary>
        public void SetDocument()
        {
            if (string.IsNullOrEmpty(Id))
                Id = fbd.GetNewDocId(Constants.collectionName);
            fbd.SetDocument(this, Constants.collectionName, Id, OnComplete);
        }

        /// <summary>
        /// Callback method triggered after a document is successfully set in Firebase.
        /// This method will invoke the <see cref="GameCreated"/> event if the document is saved successfully.
        /// </summary>
        /// <param name="task">The task representing the result of the set operation.</param>
        private void OnComplete(Task task)
        {
            if (task.IsCompletedSuccessfully)
                GameCreated.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Joins the game as a guest player. This method performs several actions:
        /// - Starts a batch operation for Firebase updates.
        /// - Sets the player type to "Guest" and clears the player's hand.
        /// - Draws 4 cards from the package and updates the player's card set.
        /// - Updates Firebase with the new game state, including setting the game as "full".
        /// - Updates the current player's name and the game status.
        /// </summary>
        public void JoinGame()
        {
            fbd.StartBatch();
            MyPlayerType = PlayerType.Guest;
            myCards.Clear();
            for (int i = 0; i < 4; i++)
            {
                myCards.Add(package.TakeCard());
            }
            myCards.FixMargin(true);
            UpdateFirebase();
            fbd.BatchUpdateField(Constants.collectionName, Id, "PackageStrings", PackageStrings);
            fbd.BatchUpdateField(Constants.collectionName, Id, "TableCardsStrings", TableCardsStrings);
            fbd.BatchUpdateField(Constants.collectionName, Id, "IsFull", true);

            GuestName = fbd.DisplayName;
            fbd.UpdateField(Constants.collectionName, Id, "GuestName", GuestName);

            fbd.BatchUpdateField(Constants.collectionName, Id, "NextPlay", 1);

            CurrentPlayers++;
            fbd.BatchIncrementField(Constants.collectionName, Id, "CurrentPlayers", 1);

            tableCards.FixMargin(false);
            fbd.CommitBatch(OnComplete);
        }

        /// <summary>
        /// Returns the text that represents the current player's status based on the game state.
        /// The returned string depends on whether the game is over, full, or if it's the player's turn.
        /// </summary>
        /// <returns>A string indicating the player's turn status (e.g., "Play", "Wait", "Game Over", "Not Full Yet").</returns>
        public string GetPlayerTurnChangedText()
        {
            string targetTurnText = string.Empty;
            if (WinnerName.Length > 0)
            {
                targetTurnText = "Game Over";
            }
            else if (!IsFull)
            {
                targetTurnText = "Not Full Yet";
            }
            else
            {
                targetTurnText = MyPlayerType == PlayingNow ? "Play" : "Wait";
            }
            return targetTurnText;
        }



        /// <summary>
        /// Displays a popup indicating the winner of the game. If the current player is the winner,
        /// their win count is incremented in the preferences. If a "Leave Game" popup is open, it is closed.
        /// </summary>
        public void ShowWinPopUp()
        {
            if (WinnerName != null && !isGameEndedPopupOpened)
            {
                if (WinnerName == myName)
                {
                    int currentGamesWon = Preferences.Get(Constants.GamesWonKey, 0);
                    Preferences.Set(Constants.GamesWonKey, currentGamesWon + 1);
                }
                isGameEndedPopupOpened = true;
                if (leaveGamePopup != null)
                {
                    leaveGamePopup.Close();
                    leaveGamePopup = null;
                }
                Shell.Current.ShowPopup(new WinGamePopUp(WinnerName, myName, true));
            }
        }

        /// <summary>
        /// Opens the "Leave Game" popup, allowing the player to exit the game. The popup is shown 
        /// and a handler is set up to close it when it's closed, cleaning up any resources afterward.
        /// </summary>
        public void OpenLeaveGamePopUp()
        {
            leaveGamePopup = new LeaveGamePopUp(this);

            leaveGamePopup.Closed += (s, e) =>
            {
                leaveGamePopup = null;
            };

            Shell.Current.ShowPopup(leaveGamePopup);
        }

        /// <summary>
        /// Displays a popup indicating that the game has ended. Checks if the game has a winner,
        /// and shows the appropriate popup message. If the current player has won, their win count is incremented.
        /// Also closes any open "Leave Game" popups before displaying the win popup.
        /// </summary>
        public void OnGameEndedPopUp()
        {
            bool isOver = false;
            if (CheckWinner())
                isOver = true;

            if (!isGameEndedPopupOpened)
            {
                isGameEndedPopupOpened = true;
                Shell.Current.ShowPopup(new WinGamePopUp(WinnerName, myName, isOver));
                Preferences.Set(Constants.GamesWonKey, Preferences.Get(Constants.GamesWonKey, 0) + 1);
            }

            if (leaveGamePopup != null)
            {
                leaveGamePopup.Close();
                leaveGamePopup = null;
            }
        }

        public async void PlayTextToSpeech()
        {
            if(ActionText != string.Empty)
                await TextToSpeech.SpeakAsync(ActionText);
        }

        #endregion
    }
}