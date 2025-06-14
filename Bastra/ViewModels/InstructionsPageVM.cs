using Bastra.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Bastra.ViewModels
{
    public class InstructionsPageVM : ObservableObject
    {
        private string _selectedItem;
        #region Properties
                public const string Hebrew = "Hebrew";
                public const string English = "English";
                public string SelectedItem
                {
                    get => _selectedItem;
                    set
                    {
                        if (_selectedItem != value)
                        {
                            _selectedItem = value;
                            OnPropertiesChanged();
                        }
                    }
                }
                public bool IsHebrew => SelectedItem == Hebrew;
                public bool IsEnglish => SelectedItem == English;
                public string TranslateTo => IsHebrew ? "Translate To English" : "תרגום לעברית";
                public string TranslateImage => IsHebrew ? "english_alpha.png" : "hebrew_alpha.png";
                public FlowDirection FlowDirection => IsHebrew ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                public string Title => IsHebrew ? " 📜 הוראות" : "Instructions 📜";

                public string Rule1Title => IsHebrew ? "📌 איך מתחילים?" : "📌 How to Start?";
                public string Rule1Content => IsHebrew ?
                    "כל שחקן מקבל 4 קלפים מהחפיסה.\n4 קלפים נוספים מונחים גלויים על השולחן." :
                    "Each player gets 4 cards from the deck.\n4 additional cards are placed face-up on the table.";

                public string Rule2Title => IsHebrew ? "🔄 מהלך המשחק" : "🔄 Turn Actions";
                public string Rule2Content => IsHebrew ?
                    "בתורך, תוכל:\n✅ לזרוק קלף מהיד שלך לשולחן.\n✅ לאסוף קלפים מהשולחן לפי הכללים." :
                    "On your turn, you can:\n✅ Discard a card from your hand.\n✅ Collect cards from the table based on the rules.";

                public string Rule3Title => IsHebrew ? "🂡 איך אוספים קלפים?" : "🂡 How to Collect Cards?";
                public string Rule3Content => IsHebrew ?
                    "🔹 מלך 👑 ← אוסף רק מלך.\n🔹 מלכה 👸 ← אוספת רק מלכה.\n🔹 נסיך 🤴 ← אוסף את כל הקלפים שעל השולחן.\n🔹 שאר הקלפים ← ניתן לאסוף קלפים שסכומם שווה לערך הקלף שבידך." :
                    "King 👑 → Can only collect another King.\nQueen 👸 → Can only collect another Queen.\nJack 🤴 → Collects all cards on the table.\nOther Cards → Can collect cards that sum up to the value of your card.";

                public string Rule4Title => IsHebrew ? "♻️ קבלת קלפים חדשים" : "♻️ Getting New Cards";
                public string Rule4Content => IsHebrew ?
                    "אם נגמרו לך הקלפים, תקבל 4 קלפים חדשים מהחפיסה." :
                    "If you run out of cards, you receive 4 new ones from the deck.";

                public string Rule5Title => IsHebrew ? "🏆 ניקוד במשחק" : "🏆 Scoring System";
        public string Rule5Content => IsHebrew
        ? "אס 🂱 ונסיך 🤴 = 1 נקודה.\n" +
        "10 יהלום ♦ = 3 נקודות.\n" +
        "2 תלתן ♣ = 2 נקודות.\n" +
        "בסטרה 🎯 = 10 נקודות.\n" +
        "הכי הרבה קלפים בסיבוב 🃏 = 7 נקודות."
        : "Ace 🂱 and Jack 🤴 = 1 point.\n" +
        "10 of Diamonds ♦ = 3 points.\n" +
        "2 of Clubs ♣ = 2 points.\n" +
        "Bastra 🎯 = 10 points.\n" +
        "Most cards in round🃏 = 7 points.";


        public string Rule6Title => IsHebrew ? "🎯 מהי בסטרה?" : "🎯 What is Bastra?";
                public string Rule6Content => IsHebrew ?
                    "בסטרה מתרחשת כאשר שחקן אוסף את כל הקלפים שעל השולחן עם קלף אחד, למעט נסיך." :
                    "A Bastra occurs when a player collects all the cards from the table using a single card, except for the Jack.";
        #endregion

        #region ICommands
        public ICommand TranslateCommand { get; }
        #endregion

        #region Constructor
        public InstructionsPageVM()
        {
            TranslateCommand = new Command(Translate);         
        }
        #endregion

        #region Functions
        private void Translate()
        {
            SelectedItem = SelectedItem == Hebrew ? English : Hebrew;
        }     
        public void OnPropertiesChanged()
        {
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(TranslateTo));
            OnPropertyChanged(nameof(IsHebrew));
            OnPropertyChanged(nameof(IsEnglish));
            OnPropertyChanged(nameof(TranslateImage));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Rule1Title));
            OnPropertyChanged(nameof(Rule1Content));
            OnPropertyChanged(nameof(Rule2Title));
            OnPropertyChanged(nameof(Rule2Content));
            OnPropertyChanged(nameof(Rule3Title));
            OnPropertyChanged(nameof(Rule3Content));
            OnPropertyChanged(nameof(Rule4Title));
            OnPropertyChanged(nameof(Rule4Content));
            OnPropertyChanged(nameof(Rule5Title));
            OnPropertyChanged(nameof(Rule5Content));
            OnPropertyChanged(nameof(Rule6Title));
            OnPropertyChanged(nameof(Rule6Content));
            OnPropertyChanged(nameof(FlowDirection));
        }
        #endregion
    }
}
