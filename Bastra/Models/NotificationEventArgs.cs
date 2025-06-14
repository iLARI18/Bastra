namespace Bastra.Models
{
    public class NotificationEventArgs : EventArgs
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = String.Empty;
    }
}
