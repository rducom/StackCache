namespace StackCache.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CacheKeys;

    /// <summary>
    /// Cache adapter interface for 2nd level cache
    /// Since it's a distributed cache, everything is async here
    /// </summary>
    public interface ICacheAdapterAsync
    {
        Task<T> Get<T>(CacheKey key);
        Task Put<T>(CacheKey key, T value);
        Task<T> GetOrCreate<T>(CacheKey key, Func<CacheKey, T> cacheValueCreator);
        Task Remove(CacheKey key);
        Task<IEnumerable<T>> GetRegion<T>(KeyPrefix prefix);
        Task PutRegion<T>(IDictionary<CacheKey, T> values);
        Task RemoveRegion(KeyPrefix prefix);
    }
}