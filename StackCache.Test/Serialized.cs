namespace StackCache.Test
{
    using ProtoBuf;

    [ProtoContract]
    public class Serialized
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Property { get; set; }
    }
}