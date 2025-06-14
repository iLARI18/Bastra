using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using Bastra.Models;

namespace Bastra.Platforms.Android
{
    public class NotificationManagerService : INotificationManagerService
    {
        const string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "The default channel for notifications.";
        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const string MessageIdKey = "msgId";
        public const string CancelKey = "cancel";
        bool channelInitialized = false;
        int messageId = 0;
        int pendingIntentId = 0;
        private readonly NotificationManagerCompat compatManager;

        public event EventHandler? NotificationReceived;
        public static NotificationManagerService? Instance { get; private set; }
        public NotificationManagerService()
        {
            if (Instance == null)
            {
                CreateNotificationChannel();
                compatManager = NotificationManagerCompat.From(Platform.AppContext);
                Instance = this;
            }
        }
        public void SendNotification(string title, string message, DateTime? notifyTime = null)
        {
            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }

            if (notifyTime != null)
            {
                Intent intent = new(Platform.AppContext, typeof(AlarmHandler));
                intent.PutExtra(TitleKey, title);
                intent.PutExtra(MessageKey, message);
                intent.PutExtra(MessageIdKey, messageId);
                intent.PutExtra(CancelKey, false);


                intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

                PendingIntentFlags pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                    ? PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable
                    : PendingIntentFlags.CancelCurrent;

                PendingIntent? pendingIntent = PendingIntent.GetBroadcast(Platform.AppContext, pendingIntentId++, intent, pendingIntentFlags);
                long triggerTime = GetNotifyTime(notifyTime.Value);
                AlarmManager? alarmManager = Platform.AppContext.GetSystemService(Context.AlarmService) as AlarmManager;
                if (pendingIntent != null)
                {
                    alarmManager?.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
                    intent.RemoveExtra(CancelKey);
                    intent.PutExtra(CancelKey, true);
                    pendingIntent = PendingIntent.GetBroadcast(Platform.AppContext, pendingIntentId++, intent, pendingIntentFlags);
                    DateTime dt = notifyTime.Value.AddSeconds(10);
                    triggerTime = GetNotifyTime(dt);
                    alarmManager?.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
                }
            }
            else
            {
                Show(title, message);
            }
        }
        public void ReceiveNotification(string title, string message)
        {
            NotificationEventArgs args = new NotificationEventArgs()
            {
                Title = title,
                Message = message,
            };
            Instance?.NotificationReceived?.Invoke(null, args);
        }
        public void Show(string title, string message)
        {
            Intent intent = new(Platform.AppContext, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
            PendingIntentFlags pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;
            PendingIntent? pendingIntent = PendingIntent.GetActivity(Platform.AppContext, pendingIntentId++, intent, pendingIntentFlags);
            NotificationCompat.Builder builder = new NotificationCompat.Builder(Platform.AppContext, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetLargeIcon(BitmapFactory.DecodeResource(Platform.AppContext.Resources, Resource.Drawable.app_logo))
                .SetSmallIcon(Resource.Drawable.app_logo);

            Notification notification = builder.Build();
            compatManager?.Notify(messageId++, notification);
        }

        public void CancelNotification(int msgId)
        {
            compatManager.Cancel(msgId);
        }

        private void CreateNotificationChannel()
        {
            // Create the notification channel, but only on API 26+.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Java.Lang.String channelNameJava = new(channelName);
                NotificationChannel channel = new(channelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = channelDescription
                };
                // Register the channel
                NotificationManager? manager = Platform.AppContext.GetSystemService(Context.NotificationService) as NotificationManager;
                manager?.CreateNotificationChannel(channel);
                channelInitialized = true;
            }
        }
        private static long GetNotifyTime(DateTime notifyTime)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            double epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            long utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / 10000;
            return utcAlarmTime; // milliseconds
        }
    }
}
