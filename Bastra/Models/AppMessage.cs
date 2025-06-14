using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Bastra.Models
{
    // A generic message wrapper used with MVVM Toolkit's messaging system.
    // This allows sending strongly-typed messages (of type T) between ViewModels/components.
    public class AppMessage<T>(T msg) : ValueChangedMessage<T>(msg)
    {

    }
}
