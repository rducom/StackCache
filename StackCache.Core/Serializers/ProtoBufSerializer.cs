namespace StackCache.Core.Serializers
{
    using System.IO;
    using ProtoBuf;

    public class ProtoBufSerializer : ISerializer
    { 
        public byte[] Serialize<T>(T item)
        {
            byte[] data;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, item);
                data = ms.ToArray();
            }
            return data;
        }

        public T Deserialize<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}