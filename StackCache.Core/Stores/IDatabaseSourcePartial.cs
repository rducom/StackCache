namespace StackCache.Core.Stores
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDatabaseSourcePartial<T, TKey> : IKeyConverter<T, TKey>
        where T : class
    {
        Task<T> Get(TKey key);
        Task Save(IEnumerable<Crud<T>> values);
    }
}