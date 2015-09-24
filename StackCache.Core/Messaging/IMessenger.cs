namespace StackCache.Core.Messaging
{
    using System;

    public interface IMessenger
    {
        void Notify(string channel, Notification notification);

        void Subscribe(string channel, Action<string, Notification> onNotification);
    }
}