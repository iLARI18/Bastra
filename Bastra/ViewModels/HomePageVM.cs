using Bastra.Models;
using Bastra.Views;
using System.Windows.Input;

namespace Bastra.ViewModels
{
    public class HomePageVM : ObservableObject
    {
        #region ICommands
        public ICommand StartJoinGamePageCommand { get; protected set; }
        #endregion

        #region Properties
        public string Name { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the ViewModel for the home page. The constructor sets up the player's name from the preferences,
        /// and initializes commands for navigating to the instructions page and starting the game.
        /// </summary>
        public HomePageVM( )
        {
            StartJoinGamePageCommand = new Command(StartJoinGamePage);
        }
        #endregion
        /// <summary>
        /// Navigates to the "Join Game" page, passing the player's name as a parameter to the new page.
        /// </summary>
        private void StartJoinGamePage()
        {
            Shell.Current.Navigation.PushAsync(new JoinGamePage(Name));
        }

    }
}
