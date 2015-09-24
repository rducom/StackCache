namespace StackCache.Core.CacheValues
{
    /// <summary>
    /// Cache value container
    /// Enables invalidation mecanism in order to force local cache to retreive new value's data from 2nd or 3rd level cache
    /// </summary>
    public interface ICacheValue
    {
        /// <summary>
        /// True is the local data differs from remote data 
        /// Invalidation occurs after a value have been updated remotely
        /// </summary>
        bool IsInvalidated { get; set; }
    }
}