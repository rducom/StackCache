namespace StackCache.Core.Stores
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CacheKeys;

    public interface IKeyConverter<T, TKey>
     where T : class
    {
        Key ToKey(TKey key);

        Key ToKey(T value);

        KeyPrefix Prefix { get; }
    }
     
    public interface IDatabaseSourceGlobal<T, TKey> : IKeyConverter<T, TKey>
        where T : class
    {
        Task<IEnumerable<T>> Load();

        Task Save(IEnumerable<Crud<T>> values);
    }

    public interface IDatabaseSourcePartial<T, TKey> : IKeyConverter<T, TKey>
    where T : class
    {
        Task<T> Get(TKey key);
        Task Save(IEnumerable<Crud<T>> values);
    }

}