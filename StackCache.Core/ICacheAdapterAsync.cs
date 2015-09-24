namespace StackCache.Core
{
    // TODO : implement the async distributed cache adapter version
    // should removes the Task<> methods on synchronous ICacheAdapter

    //using System;
    //using System.Collections.Generic;
    //using System.Threading.Tasks;
    //using CacheKeys;

    ///// <summary>
    ///// Cache adapter interface for local or distributed cache wrapper implrementation
    ///// Asynchronous version for distributed cache. Optional implementation
    ///// </summary>
    //public interface ICacheAdapterAsync
    //{
    //    Task<T> Get<T>(CacheKey key);
    //    Task Put<T>(CacheKey key, T value);
    //    Task<T> GetOrCreate<T>(CacheKey key, Func<CacheKey, T> cacheValueCreator);
    //    Task Remove(CacheKey key);

    //    Task<IEnumerable<T>> GetRegion<T>(KeyPrefix prefix);
    //    Task PutRegion<T>(IDictionary<CacheKey, T> values);
    //    Task RemoveRegion(KeyPrefix prefix);
    //}
}