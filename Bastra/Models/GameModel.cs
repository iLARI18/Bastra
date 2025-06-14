using Bastra.ModelsLogic;
using Plugin.CloudFirestore.Attributes;

namespace Bastra.Models
{
    /// <summary>
    /// Represents the full game data model used for syncing game state with Firebase.
    /// </summary>
    public class GameModel
    {
        /// <summary>
        /// Gets or sets the current player's role (local use only, ignored by Firestore).
        /// </summary>
        [Ignored] public PlayerType MyPlayerType { get; set; }

        /// <summary>
        /// Gets or sets the local game ID (ignored by Firestore).
        /// </summary>
        [Ignored] public string Id { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of players allowed in the game (local only).
        /// </summary>
        [Ignored] public int MaxPlayers = 2;
     
        /// <summary>
        /// Gets or sets the number of cards the player has picked (local only).
        /// </summary>
        [Ignored] public int PickedCardsCount { get; set; } = 0;

        /// <summary>
        /// The player's hand (not synced with Firestore).
        /// </summary>
        protected CardsSet myCards;

        /// <summary>
        /// The cards collected by the player (not synced with Firestore).
        /// </summary>
        protected CardsSet collectedCards;

        /// <summary>
        /// The remaining card deck (not synced with Firestore).
        /// </summary>
        protected CardsSet package;

        /// <summary>
        /// The cards currently visible on the table (not synced with Firestore).
        /// </summary>
        protected CardsSet tableCards;

        /// <summary>
        /// Gets or sets the name of the player who won the game.
        /// </summary>
        public string WinnerName { get; set; } = string.Empty;

        /// <summary>
        /// Enum representing the type of player.
        /// </summary>
        public enum PlayerType { Host, Guest };

        /// <summary>
        /// Gets or sets the time when the game was created.
        /// </summary>
        public DateTime Created { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the current number of players in the game.
        /// </summary>
        public int CurrentPlayers { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether the game has reached its player limit.
        /// </summary>
        public bool IsFull { get; set; }

        /// <summary>
        /// Gets or sets the player whose turn it currently is.
        /// </summary>
        public PlayerType PlayingNow { get; set; } = PlayerType.Host;

        /// <summary>
        /// Gets or sets the score required to win the game.
        /// </summary>
        public int ScoreTarget { get; set; } = 0;

        /// <summary>
        /// Gets or sets the name of the host player.
        /// </summary>
        public string HostName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the guest player.
        /// </summary>
        public string GuestName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the host's score.
        /// </summary>
        public int HostScore { get; set; } = 0;

        /// <summary>
        /// Gets or sets the guest's score.
        /// </summary>
        public int GuestScore { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of cards collected by the host.
        /// </summary>
        public int HostCardsCtr { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of cards collected by the guest.
        /// </summary>
        public int GuestCardsCtr { get; set; } = 0;
        /// <summary>
        /// Gets or sets the number of cards collected by the host in the round.
        /// </summary>
        public int HostRoundCardsCtr { get; set; } = 0;
        /// <summary>
        /// Gets or sets the number of cards collected by the guest in the round.
        /// </summary>
        public int GuestRoundCardsCtr { get; set; } = 0;
        /// <summary>
        /// Gets or sets the number of game rounds played.
        /// </summary>
        public int GameRounds { get; set; } = 1;

        public string ActionText { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameModel"/> class,
        /// sets up a full deck and a placeholder opened card.
        /// </summary>
        public GameModel()
        {
            package = new CardsSet(full: true);
        }
    }
}
