using Android.Content;

namespace Bastra.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
    public class AlarmHandler : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            if ( intent?.Extras != null)
            {
                string title = intent?.GetStringExtra(NotificationManagerService.TitleKey) ?? string.Empty;
                string message = intent?.GetStringExtra(NotificationManagerService.MessageKey) ?? string.Empty;
                bool isCancel = intent?.GetBooleanExtra(NotificationManagerService.CancelKey, false) ?? false;
                int msgId = intent?.GetIntExtra(NotificationManagerService.MessageIdKey, 0) ?? 0;
                NotificationManagerService manager = NotificationManagerService.Instance ?? new NotificationManagerService();
                if (!isCancel)
                    manager.Show(title, message);
                else
                    manager.CancelNotification(msgId);
            }
        }
    }
}
