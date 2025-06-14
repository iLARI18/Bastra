using Bastra.Models;

namespace Bastra.ModelsLogic
{
    /// <summary>
    /// Represents a clickable playing card with selection toggling and copy functionality.
    /// Inherits from <see cref="CardModel"/> and adds interactive behavior for the UI.
    /// </summary>
    public class Card : CardModel
    {
        /// <summary>
        /// Vertical offset used when a card is selected for visual feedback.
        /// </summary>
        private const int OFFSET = 25;

        /// <summary>
        /// Event triggered when the card is clicked.
        /// </summary>
        public event EventHandler? OnClick;

        /// <summary>
        /// Initializes a new instance of the <see cref="Card"/> class with a specified shape and value.
        /// </summary>
        /// <param name="shape">The suit of the card.</param>
        /// <param name="value">The value of the card (1–13).</param>
        public Card(Shapes shape, int value) : base(shape, value)
        {
            Clicked += OnCardClick;
        }

        /// <summary>
        /// Handles the card's click event and invokes the <see cref="OnClick"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        public void OnCardClick(object? sender, EventArgs e)
        {
            OnClick?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Toggles the selected state of the card and adjusts its margin
        /// to give visual feedback (e.g., lift the card up when selected).
        /// </summary>
        public void ToggleSelected()
        {
            IsSelected = !IsSelected;

            Margin = new Thickness(
                Margin.Left,
                IsSelected ? -OFFSET : 0,
                Margin.Right,
                IsSelected ? OFFSET : 0
            );
        }

        /// <summary>
        /// Creates a copy of the specified <see cref="Card"/> instance, preserving its shape, value, and index.
        /// </summary>
        /// <param name="card">The card to copy.</param>
        /// <returns>A new <see cref="Card"/> instance with the same properties.</returns>
        public static Card Copy(Card card)
        {
            Card newCard = new(Shapes.Club, 0); // Default placeholder

            if (!card.IsEmpty)
            {
                newCard = new Card(card.Shape, card.Value)
                {
                    Index = card.Index
                };
            }

            return newCard;
        }
    }
}
