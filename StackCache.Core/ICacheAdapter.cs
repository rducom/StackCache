namespace StackCache.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CacheKeys;
    using Locking;

    /// <summary>
    /// Cache adapter interface for local or distributed cache wrapper implrementation
    /// </summary>
    public interface ICacheAdapter
    { 
        bool Get<T>(CacheKey key, out T value);

        void Put<T>(CacheKey key, T value);

        T GetOrCreate<T>(CacheKey key, Func<CacheKey, T> cacheValueCreator);

        void Remove(CacheKey key);

        IEnumerable<T> GetRegion<T>(KeyPrefix prefix);

        void PutRegion<T>(KeyValuePair<CacheKey, T>[] values);

        void RemoveRegion(KeyPrefix prefix);


        Task<IEnumerable<T>> GetRegionAsync<T>(KeyPrefix prefix);

        Task<IEnumerable<KeyValuePair<CacheKey, T>>> GetRegionKeyValuesAsync<T>(KeyPrefix prefix);


        ILock GetLocker();
        void Invalidate(params CacheKey[] key);
    }
}