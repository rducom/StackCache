namespace StackCache.Core.Messaging
{
    using System.Runtime.Serialization;
    using CacheKeys;
    using ProtoBuf;
    using ProtoBuf.Meta;

    [ProtoContract]
    public class DataNotification : INotification
    {
        public DataNotification()
        {
        }

        public DataNotification(string source, NotificationType notificationType, params CacheKey[] keys)
        {
            this.Source = source;
            this.NotificationType = notificationType;
            this.Keys = keys;
        }

        [ProtoMember(1)]
        public string Source { get; set; }
        [ProtoMember(2)]
        public NotificationType NotificationType { get; set; }
        [ProtoMember(3)]
        public CacheKey[] Keys { get; set; }
    }
}