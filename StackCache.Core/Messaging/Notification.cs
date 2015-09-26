namespace StackCache.Core.Messaging
{
    using ProtoBuf;

    [ProtoContract]
    public class Notification : INotification
    {
        public Notification()
        {
        }

        public Notification(string source)
        {
            this.Source = source;
        }

        [ProtoMember(1)]
        public string Source { get; set; }
    }
}
