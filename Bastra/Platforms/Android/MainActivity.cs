using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Bastra.Models;
using Bastra.ModelsLogic;
using Bastra.Platforms.Android;
using Bastra.Utilities;
using CommunityToolkit.Mvvm.Messaging;

namespace Bastra.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private Notifications? notifications;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            Intent intent = new(this, typeof(DeleteOldDocsService));
            StartService(intent);

            base.OnCreate(savedInstanceState);

            WeakReferenceMessenger.Default.Register<AppMessage<TimerSetting>>(this, (r, m) =>
            {
                OnMessageReceived(m.Value);
            });

            PermissionStatus status = await Permissions.RequestAsync<NotificationPermission>().ContinueWith(OnComplete);

            notifications = new Notifications();
        }

        protected override void OnPause()
        {
            base.OnPause();

            Game game = Bastra.Models.GameManager.Instance.CurrentGame;

            if (Shell.Current?.CurrentPage is Bastra.Views.GamePage && game != null)
            {
                if (game.CurrentPlayers == game.MaxPlayers)
                {
                    notifications.PushNotification(
                        "Your Game Awaits!",
                        "Game is still active. Come back to finish it!",
                        DateTime.Now.AddSeconds(5)
                    );
                }
            }
        }


        private static void OnMessageReceived(TimerSetting setting)
        {
            _ = new MyCountDownTimer(setting.MillisInFuture, setting.CountDownInterval);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            if (intent != null)
                CreateNotificationFromIntent(intent);
        }

        static void CreateNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(NotificationManagerService.TitleKey) ?? string.Empty;
                string message = intent.GetStringExtra(NotificationManagerService.MessageKey) ?? string.Empty;
                INotificationManagerService? service = null;
                if (IPlatformApplication.Current != null)
                    service = IPlatformApplication.Current.Services.GetService<INotificationManagerService>();
                service?.ReceiveNotification(title, message);
            }
        }

        private PermissionStatus OnComplete(Task<PermissionStatus> task)
        {
            return task.Result;
        }
    }
}
