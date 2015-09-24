namespace StackCache.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CacheKeys;
    using Locking;


    public interface ICacheAdapter
    {
        bool Get<T>(CacheKey key, out T value);

        void Put<T>(CacheKey key, T value);

        T GetOrCreate<T>(CacheKey key, Func<CacheKey, T> cacheValueCreator);

        void Remove(CacheKey key);

        IEnumerable<T> GetRegion<T>(KeyPrefix prefix);

        void PutRegion<T>(KeyValuePair<CacheKey, T>[] values);

        void RemoveRegion(KeyPrefix prefix);

        ILock GetLocker();

        Task<IEnumerable<T>> GetRegionAsync<T>(KeyPrefix prefix);

        Task<IEnumerable<KeyValuePair<CacheKey, T>>> GetRegionKeyValuesAsync<T>(KeyPrefix prefix);

        void Invalidate(params CacheKey[] key);
    }
}