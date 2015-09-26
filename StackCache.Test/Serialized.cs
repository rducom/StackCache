namespace StackCache.Test
{
    using ProtoBuf;

    [ProtoContract]
    public class Serialized
    {
        [ProtoMember(1)]
        public string Property { get; set; }
    }
}