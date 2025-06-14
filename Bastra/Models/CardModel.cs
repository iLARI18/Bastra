namespace Bastra.Models;


/// <summary>
/// Represents a playing card in the Bastra game, displayed as an ImageButton.
/// </summary>
public class CardModel:ImageButton
{
    // A 2D array holding the image filenames for all cards in each suit
    private static readonly string[,] cardsImage = {
        {"ace_club.png","two_club.png","three_club.png","four_club.png","five_club.png","six_club.png","seven_club.png","eight_club.png","nine_club.png","ten_club.png","jack_club.png","queen_club.png","king_club.png"  },
        {"ace_diamond.png","two_diamond.png","three_diamond.png","four_diamond.png","five_diamond.png","six_diamond.png","seven_diamond.png","eight_diamond.png","nine_diamond.png","ten_diamond.png","jack_diamond.png","queen_diamond.png","king_diamond.png"  },
        {"ace_heart.png","two_heart.png","three_heart.png","four_heart.png","five_heart.png","six_heart.png","seven_heart.png","eight_heart.png","nine_heart.png","ten_heart.png","jack_heart.png","queen_heart.png","king_heart.png" },
        {"ace_spade.png","two_spade.png","three_spade.png","four_spade.png" ,"five_spade.png","six_spade.png","seven_spade.png","eight_spade.png","nine_spade.png" ,"ten_spade.png","jack_spade.png","queen_spade.png","king_spade.png"}};
     /// <summary>
        /// Enumeration of card suits.
        /// </summary>
        public enum Shapes { Club, Diamond, Heart, Spade };

        /// <summary>
        /// Gets the total number of cards in each suit.
        /// </summary>
        public static int CardsInShape => cardsImage.GetLength(1);

        /// <summary>
        /// Gets or sets the card's suit.
        /// </summary>
        public Shapes Shape { get; set; }

        /// <summary>
        /// Gets or sets the card's numeric value (1 to 13).
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets whether the card is currently selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets a custom index used for tracking.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets whether the card is considered empty or unassigned.
        /// </summary>
        public bool IsEmpty => Value == 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardModel"/> class
        /// with the specified suit and value, and sets up its image and layout.
        /// </summary>
        /// <param name="shape">The suit of the card.</param>
        /// <param name="value">The value of the card (1 to 13).</param>
        public CardModel(Shapes shape, int value)
        {
            Shape = shape;
            Value = value;

            if (value > 0)
                Source = cardsImage[(int)shape, value - 1];

            Aspect = Aspect.AspectFit;
            HorizontalOptions = new LayoutOptions(LayoutAlignment.Start, false);
            WidthRequest = 100;
        }
    }
