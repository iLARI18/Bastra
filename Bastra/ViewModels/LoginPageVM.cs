using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Bastra.Models;
using Bastra.ModelsLogic;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Bastra.ViewModels
{
    public class LoginPageVM : ObservableObject
    {
        #region Fields
        private readonly AppUser user;
        private string resultText;
        private bool isPassword = true;
        private string passwordIcon = "visibility_icon.png";
        private bool isUserSignedIn = true;
        private bool isClickedEnable = true;
        #endregion

        #region ICommands
        public ICommand SubmitCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        #endregion
        
        #region Properties
        public string ResultText
        {
            get => resultText;
            set
            {
                if (resultText != value)
                {
                    resultText = value;
                    OnPropertyChanged(nameof(ResultText));
                }
            }
        }
        // New property to support binding in XAML.
        public bool IsUserSignedIn
        {
            get => isUserSignedIn;
            set
            {
                if (isUserSignedIn != value)
                {
                    isUserSignedIn = value;
                    OnPropertyChanged(nameof(IsUserSignedIn));
                }
            }
        }
        public bool IsClickedEnable
        {
            get => isClickedEnable;
            set
            {
                if (isClickedEnable != value)
                {
                    isClickedEnable = value;
                    OnPropertyChanged(nameof(IsClickedEnable));
                }
            }
        }
        public string ActionName => user.IsRegistered ? "Sign in" : "Register";
        public string WelcomeMessage => user.IsRegistered ? "Welcome Back" : "Welcome";
        public string UserName
        {
            get => user.Name;
            set
            {
                if (user.Name != value)
                {
                    user.Name = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }
        public string Email
        {
            get => user.Email;
            set
            {
                if (user.Email != value)
                {
                    user.Email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }
        public string Password
        {
            get => user.Password;
            set
            {
                if (user.Password != value)
                {
                    user.Password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }
        public bool IsPassword
        {
            get => isPassword;
            set
            {
                if (isPassword != value)
                {
                    isPassword = value;
                    PasswordIcon = isPassword ? "visibility_icon.png" : "visibility_off_icon.png";
                    OnPropertyChanged(nameof(IsPassword));
                }
            }
        }
        public string PasswordIcon
        {
            get => passwordIcon;
            set
            {
                if (passwordIcon != value)
                {
                    passwordIcon = value;
                    OnPropertyChanged(nameof(PasswordIcon));
                }
            }
        }
        #endregion
        
        #region Constructor
        /// <summary>
        /// Initializes the ViewModel for the login page, setting up the user object for authentication and 
        /// initializing properties for user registration status and password visibility.
        /// The constructor also initializes the command for submitting the login form and toggling password visibility.
        /// </summary>
        public LoginPageVM()
        {
            user = new AppUser(OnAuthComplete);
            resultText = user.IsRegistered ? string.Empty : "Please Register";

            if (user.IsRegistered)
                IsUserSignedIn = false;
            else
                IsUserSignedIn = true;
            SubmitCommand = new Command(Submit);
            TogglePasswordVisibilityCommand = new Command(() => IsPassword = !IsPassword);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Handles the login or registration process for the user. If the user is already registered,
        /// the method attempts to sign them in. If the user is not registered, the method attempts to register them.
        /// If registration is incomplete, an error message is displayed.
        /// </summary>
        private void Submit()
        {
            if (user.IsRegistered)
            {
                IsClickedEnable = false;
                user.SignIn();
            }
            else if (!user.Register())
                ResultText = "Please fill all fields";
        }

        /// <summary>
        /// Handles the completion of the authentication process. If authentication (either sign-in or registration)
        /// is successful, the user's data is saved or properties are set accordingly. The main page of the application
        /// is then updated. If authentication fails, an error message is displayed with details of the failure.
        /// </summary>
        /// <param name="task">The task representing the result of the authentication process.</param>
        private void OnAuthComplete(Task task)
        {
            if (task.IsCompletedSuccessfully)
            {
                if (user.CurrentAction == AppUserModel.Actions.Register)
                    user.Save();
                else if (user.CurrentAction == AppUserModel.Actions.Signin)
                    user.SetProperties();

                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (Application.Current != null)
                    {
                        Application.Current.MainPage = new AppShell();
                    }
                });
            }
            else
            {
                string result = task.Exception.Message;
                int start = result.IndexOf("Reason");
                int end = result.IndexOf(')', start);
                result = $"Failed\n{result[start..end]}";
                ResultText = result;
            }
        }

        #endregion
    }
}
