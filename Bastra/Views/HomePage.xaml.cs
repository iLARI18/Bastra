using Bastra.ModelsLogic;
using Bastra.ViewModels;

namespace Bastra.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly HomePageVM hpvm;

        public HomePage()
        {
            InitializeComponent();
            hpvm = new();
            BindingContext = hpvm;
        }

    }
}
