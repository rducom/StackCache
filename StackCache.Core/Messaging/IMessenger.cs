namespace StackCache.Core.Messaging
{
    using System;

    public interface IMessenger
    {
        void Notify<T>(string channel, T notification) where T : INotification;

        void Subscribe<T>(string channel, Action<string, T> onNotification) where T : INotification;
    }
}