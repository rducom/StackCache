namespace StackCache.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CacheKeys;

    public interface ICache
    {
        T Get<T>(CacheKey key);

        void Put<T>(CacheKey key, T value);

        void Remove(CacheKey key);

        IEnumerable<T> GetRegion<T>(KeyPrefix prefix, Func<T, CacheKey> keyFromValue);

        Task<IEnumerable<T>> GetRegionAsync<T>(KeyPrefix prefix, Func<T, CacheKey> keyFromValue);

        Task<IEnumerable<KeyValuePair<CacheKey, T>>> GetRegionKeyValuesAsync<T>(KeyPrefix prefix);

        void PutRegion<T>(KeyValuePair<CacheKey, T>[] values);

        void RemoveRegion(KeyPrefix prefix);

        T GetOrCreate<T>(CacheKey key, Func<CacheKey, T> cacheValueCreator);
        
        Key Tenant { get; }
    }
}

