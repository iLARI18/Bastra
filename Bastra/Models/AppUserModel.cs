namespace Bastra.Models
{
    /// <summary>
    /// Abstract base class representing a user in the Bastra app.
    /// Provides common properties and enforces implementation of user-related actions.
    /// </summary>
    public abstract class AppUserModel
    {
        /// <summary>
        /// Enum to define the user's current intent: Registering or Signing in.
        /// </summary>
        public enum Actions { Register, Signin };

        /// <summary>
        /// Gets or sets the user's current action.
        /// </summary>
        public Actions CurrentAction { get; set; }

        /// <summary>
        /// Gets or sets the unique user ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's password. (Should be hashed in practice.)
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Indicates if the user has already completed registration.
        /// </summary>
        public abstract bool IsRegistered { get; }

        /// <summary>
        /// Validates the user's information (format, completeness, etc.).
        /// </summary>
        public abstract bool IsValid { get; }

        /// <summary>
        /// Handles the sign-in process for the user.
        /// </summary>
        public abstract void SignIn();

        /// <summary>
        /// Handles user registration logic.
        /// </summary>
        /// <returns>True if registration succeeds, otherwise false.</returns>
        public abstract bool Register();

        /// <summary>
        /// Sets or updates user properties (e.g., from database or input).
        /// </summary>
        /// <returns>True if properties were successfully set, otherwise false.</returns>
        public abstract bool SetProperties();
    }
}
