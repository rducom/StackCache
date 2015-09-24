namespace StackCache.Core.Messaging
{
    using CacheKeys;
    using ProtoBuf;

    [ProtoContract]
    public class Notification
    {
        public Notification(string source, Event e, params CacheKey[] keys)
        {
            this.Keys = keys;
            this.Event = e;
            this.Source = source;
        }

        public Notification()
        {
        }

        [ProtoMember(1)]
        public CacheKey[] Keys { get; set; }

        [ProtoMember(2)]
        public Event Event { get; set; }

        [ProtoMember(3)]
        public string Source { get; set; }
    }
}
