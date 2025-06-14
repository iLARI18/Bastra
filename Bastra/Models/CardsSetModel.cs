using Bastra.ModelsLogic;

namespace Bastra.Models
{
    /// <summary>
    /// Represents a collection of cards in the game (e.g., player's hand, table deck).
    /// Provides basic management of card count and selection behavior.
    /// </summary>
    public class CardsSetModel
    {
        /// <summary>
        /// Protected list holding the actual card objects in the set.
        /// </summary>
        protected readonly List<Card> cards;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardsSetModel"/> class.
        /// </summary>
        public CardsSetModel()
        {
            cards = [];
        }

        /// <summary>
        /// Gets or sets a value indicating whether only one card can be selected at a time.
        /// </summary>
        public bool SingleSelect { protected get; set; }

        /// <summary>
        /// Gets the number of cards currently in the set.
        /// </summary>
        public int Count
        {
            get { return cards.Count; }
        }
    }
}
