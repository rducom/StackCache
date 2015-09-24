namespace StackCache.Core.CacheValues
{
    public interface ICacheValue
    {
        bool IsInvalidated { get; set; }
    }
}