namespace Bastra.Models
{
    /// <summary>
    /// Interface for managing local notifications in the app.
    /// Supports sending and receiving notifications, and handling notification events.
    /// </summary>
    public interface INotificationManagerService
    {
        /// <summary>
        /// Event triggered when a notification is received.
        /// </summary>
        event EventHandler NotificationReceived;

        /// <summary>
        /// Sends a local notification with an optional scheduled time.
        /// </summary>
        /// <param name="title">The title of the notification.</param>
        /// <param name="message">The body message of the notification.</param>
        /// <param name="notifyTime">The optional time to schedule the notification.</param>
        void SendNotification(string title, string message, DateTime? notifyTime = null);

        /// <summary>
        /// Handles the logic when a notification is received.
        /// </summary>
        /// <param name="title">The title of the received notification.</param>
        /// <param name="message">The body message of the received notification.</param>
        void ReceiveNotification(string title, string message);
    }
}
