namespace StackCache.Core.Serializers
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T item);
        T Deserialize<T>(byte[] bytes);
    }
}