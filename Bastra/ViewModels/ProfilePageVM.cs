using Bastra.Models;
using Bastra.ModelsLogic;
using Bastra.Utilities;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Windows.Input;

namespace Bastra.ViewModels
{
    public class ProfilePageVM : ObservableObject
    {
        #region Fields
        private readonly FbData fbData;
        private const string SavedImagePathKey = "ProfileImagePath";
        private const string DefaultImageName = "default_profile_icon.png";
        private string title;
        private string name;
        private string dateCreated;
        private int gamesPlayed;
        private int gamesWon;
        private string lastGamePlayedDate;
        private int longestWinStreak;
        private int winStreak;
        private double progress;
        private ImageSource profileImageSource;
        #endregion
        #region Properties
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public string DateCreted
        {
            get => dateCreated;
            set
            {
                if (dateCreated != value)
                {
                    dateCreated = value;
                    OnPropertyChanged(nameof(DateCreted));
                }
            }
        }
        public int GamesPlayed
        {
            get => gamesPlayed;
            set
            {
                if (gamesPlayed != value)
                {
                    gamesPlayed = value;
                    UpdateProgress();
                    OnPropertyChanged(nameof(GamesPlayed));
                }
            }
        }
        public int GamesWon
        {
            get => gamesWon;
            set
            {
                if (gamesWon != value)
                {
                    gamesWon = value;
                    UpdateProgress();
                    OnPropertyChanged(nameof(GamesWon));
                }
            }
        }
        public string LastGamePlayedDate
        {
            get => lastGamePlayedDate;
            set
            {
                if (lastGamePlayedDate != value)
                {
                    lastGamePlayedDate = value;
                    OnPropertyChanged(nameof(LastGamePlayedDate));
                }
            }
        }
        public int LongestWinStreak
        {
            get => longestWinStreak;
            set
            {
                if (longestWinStreak != value)
                {
                    longestWinStreak = value;
                    OnPropertyChanged(nameof(LongestWinStreak));
                }
            }
        }
        public int WinStreak
        {
            get => winStreak;
            set
            {
                if (winStreak != value)
                {
                    winStreak = value;
                    OnPropertyChanged(nameof(WinStreak));
                }
            }
        }
        public double Progress
        {
            get => progress;
            set
            {
                if (progress != value)
                {
                    progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }
        public string ProgressText => GamesPlayed > 0 ? $"{(Progress * 100):F1}% Win Rate" : "No games played yet";
        public ImageSource ProfileImageSource
        {
            get => profileImageSource;
            set
            {
                if (profileImageSource != value)
                {
                    profileImageSource = value;
                    OnPropertyChanged(nameof(ProfileImageSource));
                }
            }
        }
        public bool IsCustomImageSet
        {
            get
            {
                string savedPath = Preferences.Get(SavedImagePathKey, string.Empty);
                return !string.IsNullOrWhiteSpace(savedPath) && File.Exists(savedPath);
            }
        }
        #endregion
        #region ICommand
        public ICommand PickImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        #endregion
        #region Constructor
        /// <summary>
        /// Initializes the ViewModel for the profile page. It sets up the user's profile details, including their
        /// name, account creation date, and profile image. It also fetches the user's stats and initializes commands
        /// for picking and removing the profile image.
        /// </summary>
        public ProfilePageVM()
        {
            this.fbData = new FbData();

            Name = Preferences.Get(Constants.NameKey, string.Empty);
            DateCreted = Preferences.Get(Constants.AccountCreated, string.Empty);

            Title = $"{Name}'s Profile";
            FetchUserStatsAsync();

            string savedImagePath = Preferences.Get(SavedImagePathKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(savedImagePath) && File.Exists(savedImagePath))
            {
                ProfileImageSource = ImageSource.FromFile(savedImagePath);
            }
            else
            {
                ProfileImageSource = DefaultImageName;
            }

            PickImageCommand = new Command(async () => await PickAndSetProfileImage());
            RemoveImageCommand = new Command(RemoveProfileImage);
        }
        #endregion
        #region Functions
        /// <summary>
        /// Allows the user to pick a profile photo from their device and set it as their profile image.
        /// If the photo is successfully selected, it is copied to the local folder and the profile image is updated.
        /// The path of the selected image is saved in preferences for future use. 
        /// The method also handles errors related to unsupported features or missing permissions.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task PickAndSetProfileImage()
        {
            try
            {
                FileResult? photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Please pick a profile photo"
                });

                if (photo != null)
                {
                    string newFilePath = await CopyFileToLocalFolder(photo);
                    ProfileImageSource = ImageSource.FromFile(newFilePath);
                    Preferences.Set(SavedImagePathKey, newFilePath);
                    await Toast.Make("Profile picture updated!", ToastDuration.Short).Show();
                    OnPropertyChanged(nameof(IsCustomImageSet));
                }
                else
                {
                    await Toast.Make("No photo selected.", ToastDuration.Short).Show();
                }
            }
            catch (FeatureNotSupportedException)
            {
                await Toast.Make("Feature not supported on device.", ToastDuration.Short).Show();
            }
            catch (PermissionException)
            {
                await Toast.Make("Permissions not granted.", ToastDuration.Short).Show();
            }
            catch (System.Exception ex)
            {
                await Toast.Make($"Something went wrong: {ex.Message}", ToastDuration.Long).Show();
            }
        }


        /// <summary>
        /// Copies the selected photo file from the device to the app's local storage directory.
        /// The method saves the photo to the app's data directory and returns the new file path.
        /// </summary>
        /// <param name="photo">The photo file selected by the user.</param>
        /// <returns>A task that represents the asynchronous operation. The result contains the new file 
        private async Task<string> CopyFileToLocalFolder(FileResult photo)
        {
            string appDataPath = FileSystem.AppDataDirectory;
            string fileName = Path.GetFileName(photo.FullPath);
            string newFilePath = Path.Combine(appDataPath, fileName);

            using Stream sourceStream = await photo.OpenReadAsync();
            using FileStream localFileStream = File.OpenWrite(newFilePath);
            await sourceStream.CopyToAsync(localFileStream);

            return newFilePath;
        }

        /// <summary>
        /// Removes the custom profile image if set, reverting to default.
        /// </summary>
        /// <summary>
        /// Removes the user's profile image, resetting it to the default image.
        /// The path to the image is also removed from preferences.
        /// </summary>
        private async void RemoveProfileImage()
        {
            Preferences.Remove(SavedImagePathKey);
            ProfileImageSource = DefaultImageName;
            await Toast.Make("Profile image removed!", ToastDuration.Short).Show();
            OnPropertyChanged(nameof(IsCustomImageSet));
        }

        /// <summary>
        /// Fetches the user's game statistics from preferences and updates the relevant properties.
        /// The statistics include the number of games played, games won, the date of the last game played,
        /// the longest win streak, and the current win streak.
        /// </summary>
        private void FetchUserStatsAsync()
        {
            GamesPlayed = Preferences.Get(Constants.GamesPlayedKey, 0);
            GamesWon = Preferences.Get(Constants.GamesWonKey, 0);
            LastGamePlayedDate = Preferences.Get(Constants.LastGamePlayedDateKey, "None");
            LongestWinStreak = Preferences.Get(Constants.LongestWinStreakKey, 0);
            WinStreak = Preferences.Get(Constants.WinStreakKey, 0);

            OnPropertyChanged(nameof(GamesPlayed));
            OnPropertyChanged(nameof(GamesWon));
            OnPropertyChanged(nameof(LastGamePlayedDate));
            OnPropertyChanged(nameof(LongestWinStreak));
            OnPropertyChanged(nameof(WinStreak));
            OnPropertyChanged(nameof(Progress));
        }

        /// <summary>
        /// Updates the progress of the user's game statistics by calculating the win percentage based on the games played and won.
        /// The progress value is updated and the corresponding UI text is refreshed.
        /// </summary>
        private void UpdateProgress()
        {
            Progress = GamesPlayed > 0 ? (double)GamesWon / GamesPlayed : 0;
            OnPropertyChanged(nameof(ProgressText));
        }
        #endregion
    }
}
