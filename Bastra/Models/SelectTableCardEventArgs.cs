using Bastra.ModelsLogic;

namespace Bastra.Models
{
    public class SelectTableCardEventArgs : EventArgs
    {
        public Card? SelectedCard { get; set; }
    }
}
