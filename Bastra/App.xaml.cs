using Bastra.Views;

namespace Bastra
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new LoginPage();
        }
    }
}
