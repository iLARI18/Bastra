using Bastra.Models;

namespace Bastra.Utilities
{
    public class Notifications : ObservableObject
    {
        private readonly INotificationManagerService? notificationManager;
        public NotificationEventArgs? NotificationArgs { get; set; }
        public event EventHandler<NotificationEventArgs>? NotificationReceived;
        public static PermissionStatus PermissionStatus
        {
            get
            {
                return Permissions.CheckStatusAsync<Permissions.PostNotifications>().Result;
            }
        }
        public Notifications()
        {
            notificationManager = Application.Current?.MainPage?.Handler?.MauiContext?.Services.GetService<INotificationManagerService>();
            if (notificationManager != null)
                notificationManager.NotificationReceived += OnNotificationReceived;
        }

        private void OnNotificationReceived(object? sender, EventArgs e)
        {
            NotificationArgs = (NotificationEventArgs)e;
            NotificationReceived?.Invoke(this, NotificationArgs);
        }
        public bool PushNotification(string title, string message, DateTime? notifyTime = null)
        {
            bool sent = false;
            if (notificationManager != null && PermissionStatus == PermissionStatus.Granted)
            {
                notificationManager.SendNotification(title, message, notifyTime);
                sent = true;
            }
            return sent;
        }
    }
}
