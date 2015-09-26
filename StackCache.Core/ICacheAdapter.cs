namespace StackCache.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CacheKeys;
    using Locking;

    /// <summary>
    /// Cache adapter interface for local or distributed cache wrapper implementation
    /// </summary>
    public interface ICacheAdapter
    {
        /// <summary>
        /// Returns local or distribute
        /// </summary>
        CacheType CacheType { get; }

        /// <summary>
        /// Get a single value by it's key
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        /// <returns>true if value present, else false</returns>
        bool Get<T>(CacheKey key, out T value);

        /// <summary>
        /// Put (Create or Update if exists) a value with a key
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        void Put<T>(CacheKey key, T value);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="cacheValueCreator"></param>
        /// <returns></returns>
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