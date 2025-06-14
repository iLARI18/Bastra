using Bastra.ModelsLogic;

namespace Bastra.Models
{
    /// <summary>
    /// Singleton class responsible for managing the current game instance.
    /// Ensures that only one game is managed at a time throughout the app.
    /// </summary>
    public class GameManager
    {
        /// <summary>
        /// Static instance of the <see cref="GameManager"/> (singleton pattern).
        /// </summary>
        private static GameManager? instance;

        /// <summary>
        /// Provides global access to the single <see cref="GameManager"/> instance.
        /// Initializes it only once when first accessed.
        /// </summary>
        public static GameManager Instance => instance ??= new GameManager();

        /// <summary>
        /// Gets or sets the currently active game.
        /// </summary>
        public Game CurrentGame { get; set; }

        /// <summary>
        /// Private constructor to prevent external instantiation.
        /// </summary>
        private GameManager() { }
    }
}
