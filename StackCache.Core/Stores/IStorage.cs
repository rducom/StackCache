namespace StackCache.Core.Stores
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IStorage<T, TKey> where T : class
    {
        Task<T> Get(TKey key);
        Task<IEnumerable<T>> Get(IEnumerable<TKey> keys);
        Task<IEnumerable<T>> GetAll();
        Task Save(IEnumerable<Crud<T>> value);
    }
}