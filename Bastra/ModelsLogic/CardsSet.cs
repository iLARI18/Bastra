using Bastra.Models;
namespace Bastra.ModelsLogic;

// Represents a collection of interactive cards with logic for selection, display, and animations.
public class CardsSet : CardsSetModel
{
    /// <summary>
    /// Random generator used for selecting random cards.
    /// </summary>
    private readonly Random rnd;

    /// <summary>
    /// Gets or sets the currently selected card.
    /// </summary>
    public Card selectedCard { get; set; }

    /// <summary>
    /// A placeholder card used to represent "no selection".
    /// </summary>
    private readonly Card emptyCard;

    /// <summary>
    /// Converts the set of cards to a list of string representations (e.g., "Heart | 7").
    /// </summary>
    public List<string> ToStringArray => cards.Select(card => $"{card.Shape} | {card.Value}").ToList();

    /// <summary>
    /// Initializes a new instance of the <see cref="CardsSet"/> class.
    /// Optionally fills the set with a full deck of 52 cards.
    /// </summary>
    /// <param name="full">Whether to initialize the set with a full deck.</param>
    public CardsSet(bool full) : base()
    {
        emptyCard = new Card(CardModel.Shapes.Diamond, 0);
        selectedCard = emptyCard;
        rnd = new Random();
        if (full)
            FillPakage();
    }

    /// <summary>
    /// Adds all 52 cards (4 suits × 13 values) to the set.
    /// </summary>
    public void FillPakage()
    {
        foreach (CardModel.Shapes shape in Enum.GetValues(typeof(CardModel.Shapes)))
            for (int value = 1; value <= Card.CardsInShape; value++)
                cards.Add(new Card(shape, value));
    }

    /// <summary>
    /// Adds a card to the set and sets its margin based on its position in the collection.
    /// </summary>
    /// <param name="card">The card to add.</param>
    /// <returns>The added card.</returns>
    public Card Add(Card card)
    {
        card.Index = cards.Count;
        card.Margin = new Thickness(50 + 30 * cards.Count, 0, 0, 0);
        cards.Add(card);
        return card;
    }

    /// <summary>
    /// Randomly takes a card from the set and removes it.
    /// </summary>
    /// <returns>The selected card, or an empty card if the set is empty.</returns>
    public Card TakeCard()
    {
        Card card = new(CardModel.Shapes.Club, 0);
        if (cards.Count > 0)
        {
            int index = rnd.Next(0, cards.Count);
            card = cards[index];
            cards.RemoveAt(index);
        }
        return card;
    }

    /// <summary>
    /// Toggles the selection state of a card. Supports single and multiple selection modes.
    /// </summary>
    /// <param name="card">The card to select or deselect.</param>
    public void SelectCard(Card card)
    {
        if (SingleSelect)
        {
            if (card.IsSelected)
            {
                selectedCard = emptyCard;
                card?.ToggleSelected();
            }
            else
            {
                selectedCard?.ToggleSelected();
                card.ToggleSelected();
                selectedCard = card;
            }
        }
        else
        {
            card?.ToggleSelected();
        }
    }

    /// <summary>
    /// Removes a specific selected card from the set and hides it from view.
    /// </summary>
    /// <param name="comparedCard">The card to remove.</param>
    /// <returns>The removed card.</returns>
    public Card RemoveSelectedCard(Card comparedCard)
    {
        if (comparedCard == null || comparedCard.IsEmpty)
            return comparedCard;

        comparedCard.IsSelected = false;
        comparedCard.IsVisible = false;

        if (comparedCard.Index >= 0 && comparedCard.Index < cards.Count)
        {
            cards.RemoveAt(comparedCard.Index);
            FixMargin(true);
        }

        return comparedCard;
    }

    /// <summary>
    /// Returns a list of all cards in the set that are currently selected.
    /// </summary>
    /// <returns>A list of selected cards.</returns>
    public List<Card> GetSelectedCards()
    {
        List<Card> selectedCards = new();
        foreach (Card card in cards)
        {
            if (card.IsSelected)
                selectedCards.Add(card);
        }
        return selectedCards;
    }

    /// <summary>
    /// Gets a list of all cards currently in the set.
    /// </summary>
    public List<Card> CardSetToList => cards;

    /// <summary>
    /// Removes all selected cards from the set and returns them.
    /// </summary>
    /// <returns>A list of the removed cards.</returns>
    public List<Card> RemoveSelectedCards()
    {
        List<Card> selectedCards = GetSelectedCards();
        foreach (Card card in selectedCards)
        {
            card.IsSelected = false;
            card.IsVisible = false;
            cards.Remove(card);
        }
        FixMargin(false);
        return selectedCards;
    }


    /// <summary>
    /// Throws a card from the set: removes it and returns a copy.
    /// The original card is hidden and deselected. Margins are fixed afterward.
    /// </summary>
    /// <param name="cardToThrow">The card to be thrown.</param>
    /// <returns>A copy of the thrown card.</returns>
    public Card ThrowCard(Card cardToThrow)
    {
        Card card = Card.Copy(cardToThrow);

        if (!cardToThrow.IsEmpty)
        {
            cardToThrow.IsSelected = false;
            cardToThrow.IsVisible = false;

            if (cardToThrow.Index < cards.Count)
                cards.RemoveAt(cardToThrow.Index);
            else
                cards.RemoveAt(cardToThrow.Index - 1);

            cards.Remove(card); // Extra safety
            FixMargin(true);
        }
        return card;
    }

    /// <summary>
    /// Makes all cards in the set visible.
    /// Useful after hiding cards during gameplay logic.
    /// </summary>
    public void MakeVisible()
    {
        foreach (Card card in cards)
            card.IsVisible = true;
    }

    /// <summary>
    /// Rebuilds the card set from a list of string representations.
    /// Each string should be in the format "Shape | Value".
    /// </summary>
    /// <param name="cardStrings">The list of card strings to parse and add.</param>
    public void FromStringArray(List<string> cardStrings)
    {
        cards.Clear();
        foreach (string cardString in cardStrings)
        {
            string[] parts = cardString.Split('|');
            if (parts.Length == 2 && Enum.TryParse(parts[0].Trim(), out CardModel.Shapes shape) && int.TryParse(parts[1].Trim(), out int value))
            {
                cards.Add(new Card(shape, value));
            }
        }
    }

    /// <summary>
    /// Clears all cards from the set.
    /// </summary>
    public void Clear()
    {
        cards.Clear();
    }

    /// <summary>
    /// Recalculates the layout (margin and scale) of the cards in the set.
    /// This determines how the cards are visually arranged in the UI.
    /// </summary>
    /// <param name="IsMyCards">
    /// If true, the cards are arranged as the player's hand;
    /// if false, the layout adapts based on the number of cards (used for opponents or table).
    /// </param>
    public void FixMargin(bool IsMyCards)
    {
        if (IsMyCards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].Index = i;
                cards[i].Scale = 0.9;
                cards[i].Margin = new Thickness(75 * i, 0, 0, 0);
            }
        }
        else
        {
            if (cards.Count <= 4)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    cards[i].Index = i;
                    cards[i].Scale = 0.9;
                    cards[i].Margin = new Thickness(75 * i, 0, 0, 0);
                }
            }
            else if (cards.Count <= 5)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    cards[i].Index = i;
                    cards[i].Scale = 0.80;
                    cards[i].Margin = new Thickness(6 + 70 * i, 0, 0, 0);
                }
            }
            else if (cards.Count <= 6)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    cards[i].Index = i;
                    cards[i].Scale = 0.80;
                    cards[i].Margin = new Thickness(12 + 50 * i, 0, 0, 0);
                }
            }
            else
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    cards[i].Index = i;
                    cards[i].Scale = 0.80;
                    cards[i].Margin = new Thickness(50 * i, 0, 0, 0);
                }
            }
        }
    }


    // Checks if a card is already in the set (avoids duplicates)
    /// <summary>
    /// Checks if the specified card can be added to the set without duplicating an existing card.
    /// </summary>
    /// <param name="cardToAdd">The card to validate.</param>
    /// <returns>
    /// True if no card with the same shape and value exists in the set; otherwise, false.
    /// </returns>
    public bool CanAddToMyCards(Card cardToAdd)
    {
        foreach (Card tableCard in cards)
        {
            if (tableCard.Shape == cardToAdd.Shape && tableCard.Value == cardToAdd.Value)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Performs an animation for selecting or deselecting cards on the table.
    /// Used primarily for the "Jack stealing" move mechanic.
    /// </summary>
    /// <param name="isJeckSelected">Indicates whether the Jack card was selected.</param>
    /// <param name="myCards">The player's card set, which is temporarily disabled during animation.</param>
    /// <returns>The number of cards processed during the animation.</returns>
    public async Task<int> Animation(bool isJeckSelected, CardsSet myCards)
    {
        int ctr = 0;

        // Disable all cards before animation
        foreach (Card tableCard in cards)
            tableCard.IsEnabled = false;
        foreach (Card card in myCards.CardSetToList)
            card.IsEnabled = false;

        // Select or deselect cards during animation
        if (isJeckSelected)
        {
            foreach (Card tableCard in cards)
            {
                if (!tableCard.IsSelected)
                {
                    SelectCard(tableCard);
                    tableCard.OnCardClick(tableCard, EventArgs.Empty);
                    await Task.Delay(150);
                }
                ctr++;
            }
        }
        else
        {
            foreach (Card tableCard in cards)
            {
                if (tableCard.IsSelected)
                {
                    SelectCard(tableCard);
                    tableCard.OnCardClick(tableCard, EventArgs.Empty);
                    await Task.Delay(150);
                }
                ctr++;
            }

            foreach (Card card in cards)
                card.IsEnabled = true;
        }

        foreach (Card card in myCards.CardSetToList)
            card.IsEnabled = true;

        return ctr;
    }

    /// <summary>
    /// Enables all cards in the set for interaction.
    /// Useful after disabling cards during animations or logic checks.
    /// </summary>
    public void MakeAllEnable()
    {
        foreach (Card card in cards)
            card.IsEnabled = true;
    }

    /// <summary>
    /// Calculates and returns the total value of all currently selected cards in the set.
    /// </summary>
    /// <returns>The sum of the values of selected cards.</returns>
    public int GetSelectedCardsSum()
    {
        int sum = 0;
        foreach (Card card in cards)
            if (card.IsSelected)
                sum += card.Value;
        return sum;
    }

    /// <summary>
    /// Gets the total number of cards in the set that are currently selected.
    /// </summary>
    /// <returns>The count of selected cards.</returns>
    public int GetSelectedCardsCtr()
    {
        int ctr = 0;
        foreach (Card card in cards)
            if (card.IsSelected)
                ctr++;
        return ctr;
    }

}
