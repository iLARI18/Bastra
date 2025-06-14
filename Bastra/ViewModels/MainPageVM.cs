using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Bastra.Models;
using Bastra.ModelsLogic;
using Bastra.Views;


namespace Bastra.ViewModels
{
    public class MainPageVM : ObservableObject
    {
        /*private readonly MainPage mainPage;
        private readonly AppUser user;
        private string resultText;

        public ICommand SubmitCommand { get; }

        public string ResultText
        {
            get => resultText;
            set
            {
                if (resultText != value)
                {
                    resultText = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool UserNotRegisterd
        {
            get => !user.IsRegistered;
        }
        public string ActionName
        {
            get => user.IsRegistered ? "Sign in" : "Register";
        }

        public string UserName
        {
            get => user.Name;
            set => user.Name = value;
        }
        public string Email
        {
            get => user.Email;
            set => user.Email = value;
        }
        public string Password
        {
            get => user.Password;
            set => user.Password = value;
        }

        public MainPageVM(MainPage mainPage)
        {
            this.mainPage = mainPage;
            user = new AppUser(OnAuthComplete);
            resultText = user.IsRegistered ? "Please Sign in" : "Please Register";
            SubmitCommand = new Command(Submit);
        }

        private void Submit()
        {
            if (user.IsRegistered)
            {
                user.SignIn();
                //mainPage.Navigation.PushAsync(new HomePage(user), true);
            }
            else if (!user.Register())
                ResultText = "Please fill all fields";
        }

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
                    mainPage.Navigation.PushAsync(new HomePage(), true);
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
        }*/
    }
}
