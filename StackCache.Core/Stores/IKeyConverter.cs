namespace StackCache.Core.Stores
{
    using CacheKeys;

    public interface IKeyConverter<T, TKey>
     where T : class
    {
        Key ToKey(TKey key);

        Key ToKey(T value);

        KeyPrefix Prefix { get; }
    }
}