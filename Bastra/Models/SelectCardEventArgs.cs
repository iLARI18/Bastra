using Bastra.ModelsLogic;

namespace Bastra.Models
{
    public class SelectCardEventArgs:EventArgs
    {
        public Card? SelectedCard { get; set; }
    }
}
