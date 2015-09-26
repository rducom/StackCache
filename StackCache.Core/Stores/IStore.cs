namespace StackCache.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CacheKeys;

    public interface IStore<T, in TKey> where T : class
    {
        Task<T> Get(TKey key);
        Task<IEnumerable<T>> Get(params TKey[] keys);
        Task<IEnumerable<T>> GetAll();
        Task Save(IEnumerable<Crud<T>> value);
    }
}