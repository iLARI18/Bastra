using Bastra.Models;
using Bastra.Utilities;
using System;
using System.Threading.Tasks;

namespace Bastra.ModelsLogic
{
    /// <summary>
    /// Represents a concrete app user and implements authentication and user data logic.
    /// Inherits from <see cref="AppUserModel"/> and interacts with Firebase through <see cref="FbData"/>.
    /// </summary>
    public class AppUser : AppUserModel
    {
        /// <summary>
        /// Firebase data handler for authentication and user info.
        /// </summary>
        private readonly FbData fbd;

        /// <summary>
        /// Callback to handle the result of asynchronous Firebase operations.
        /// </summary>
        private readonly Action<Task> OnComplete;

        /// <summary>
        /// Flag indicating whether the user is already registered.
        /// </summary>
        private bool registered;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppUser"/> class from saved preferences,
        /// and sets up the Firebase data handler.
        /// </summary>
        /// <param name="onComplete">The action to execute upon task completion.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="onComplete"/> is null.</exception>
        public AppUser(Action<Task> onComplete)
        {
            OnComplete = onComplete ?? throw new ArgumentNullException(nameof(onComplete));
            fbd = new FbData();

            // Load saved user data
            Email = Preferences.Get(Constants.EmailKey, string.Empty);
            Password = Preferences.Get(Constants.PasswordKey, string.Empty);
            Name = Preferences.Get(Constants.NameKey, string.Empty);

            // Set account creation date if not already set
            if (Preferences.Get(Constants.AccountCreated, string.Empty) == string.Empty)
                Preferences.Set(Constants.AccountCreated, DateTime.Now.ToString("dd/MM/yyyy"));

            // Determine if user is registered based on stored email
            registered = !string.IsNullOrWhiteSpace(Email);
        }

        /// <summary>
        /// Registers a new user using Firebase.
        /// </summary>
        /// <returns>True if registration was initiated successfully; otherwise, false.</returns>
        public override bool Register()
        {
            if (!IsValid) return false;

            CurrentAction = Actions.Register;
            try
            {
                fbd.CreateUserWithEmailAndPasswordAsync(Email, Password, Name, OnComplete);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Signs in the user using saved Firebase credentials.
        /// </summary>
        public override void SignIn()
        {
            CurrentAction = Actions.Signin;
            try
            {
                fbd.SignInWithEmailAndPasswordAsync(Email, Password, OnComplete);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sign-in failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets user properties (e.g., ID, name) from Firebase.
        /// </summary>
        /// <returns>True if properties were successfully set; otherwise, false.</returns>
        public override bool SetProperties()
        {
            try
            {
                Name = fbd.DisplayName;
                Id = fbd.UserId;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set properties: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user's information is valid (not empty).
        /// </summary>
        public override bool IsValid =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Password);

        /// <summary>
        /// Gets a value indicating whether the user is registered.
        /// </summary>
        public override bool IsRegistered => registered;

        /// <summary>
        /// Saves user credentials and related data to preferences.
        /// </summary>
        public void Save()
        {
            Preferences.Set(Constants.EmailKey, Email);
            Preferences.Set(Constants.IDKey, Id);
            Preferences.Set(Constants.PasswordKey, Password);
            Preferences.Set(Constants.NameKey, Name);
            registered = true;
        }
    }
}
