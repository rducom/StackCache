namespace StackCache.Core.Stores
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDatabaseSourceGlobal<T, TKey> : IKeyConverter<T, TKey>
        where T : class
    {
        Task<IEnumerable<T>> Load();

        Task Save(IEnumerable<Crud<T>> values);
    }
}