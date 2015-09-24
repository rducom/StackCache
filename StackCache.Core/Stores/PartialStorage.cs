namespace StackCache.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CacheKeys;

    public abstract class PartialStorage<T, TKey> : IStorage<T, TKey>
        where T : class
    {
        private readonly ICache _cache;
        private readonly IDatabaseSourcePartial<T, TKey> _source;
        private readonly IKeyConverter<T, TKey> _keyConverter;

        protected PartialStorage(ICache cache, IDatabaseSourcePartial<T, TKey> source, IKeyConverter<T, TKey> keyConverter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keyConverter == null) throw new ArgumentNullException(nameof(keyConverter));
            this._cache = cache ?? Cache.Default;
            this._source = source;
            this._keyConverter = keyConverter;
        }

        public async Task<T> Get(TKey key)
        {
            CacheKey cacheKey = this._keyConverter.Prefix + this._keyConverter.ToKey(key);
            var value = this._cache.Get<T>(cacheKey);
            if (value != null)
                return value;
            value = await this._source.Get(key);
            if (value != null)
                this._cache.Put(cacheKey, value);
            return value;
        }

        public async Task<IEnumerable<T>> Get(IEnumerable<TKey> keys)
        {
            List<Task<T>> getters = keys.Select(this.Get).ToList();
            await Task.WhenAll(getters);
            return getters.Select(i => i.Result);
        }

        public Task<IEnumerable<T>> GetAll()
        {
            IEnumerable<T> region = this._cache.GetRegion<T>(this._keyConverter.Prefix, i => this._keyConverter.Prefix + this._keyConverter.ToKey(i));
            return Task.FromResult(region);
        }

        public async Task Save(IEnumerable<Crud<T>> value)
        {
            IEnumerable<Crud<T>> enumerable = value as IList<Crud<T>> ?? value.ToList();
            await this._source.Save(enumerable);
            foreach (Crud<T> v in enumerable)
            {
                switch (v.Action)
                {
                    case CrudAction.Insert:
                    case CrudAction.Update:
                        this._cache.Put(this._keyConverter.Prefix + this._keyConverter.ToKey(v.Value), v.Value);
                        break;
                    case CrudAction.Delete:
                        this._cache.Remove(this._keyConverter.Prefix + this._keyConverter.ToKey(v.Value));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}