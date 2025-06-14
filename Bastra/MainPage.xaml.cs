using Bastra.ViewModels;
namespace Bastra
{
    public partial class MainPage : ContentPage
    {
        public MainPage() 
        {
            InitializeComponent();
            BindingContext = new MainPageVM();
        }
    }
}
