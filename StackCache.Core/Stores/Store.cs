namespace StackCache.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using CacheKeys;
    using Election;
    using Helpers;
    using Locking;

    /// <summary>
    /// Provide a global storage for a given Type.
    /// A Storage is associated to a StackCache Region and should contains all instances of a given type
    /// </summary>
    /// <typeparam name="T">Type of the stored data</typeparam>
    /// <typeparam name="TKey">Type of the key of the stored data</typeparam>
    public abstract class Store<T, TKey> : IStore<T, TKey>
        where T : class
    {
        private readonly ICache _cache;

        private readonly AsyncLazy<IEnumerable<T>> _loaderAsyncLazy;
        private readonly IDatabaseSourceGlobal<T, TKey> _source;
        private readonly IElection _elector;

        private bool _isLoaded;

        // TODO : le prefixe doit rester ici (pas sur IDatabaseSourceGlobal)

        protected Store(ICache cache, IDatabaseSourceGlobal<T, TKey> source, IElection elector)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (source == null) throw new ArgumentNullException(nameof(source));
            Debug.Assert(cache != null, "cache != null");
            Debug.Assert(source != null, "cache != null");
            this._cache = cache;
            this._source = source;
            this._elector = elector;
            this._loaderAsyncLazy = new AsyncLazy<IEnumerable<T>>((Func<Task<IEnumerable<T>>>) this._source.Load);
            this.StoreIdentifier = typeof (T).Name;
        }

        protected virtual TimeSpan LoadingLockTimeout => TimeSpan.FromMinutes(1);

        public async Task<T> Get(TKey key)
        {
            await this.EnsureLoaded();
            return this._cache.Get<T>(this._source.Prefix + this._source.ToKey(key));
        }

        public async Task<IEnumerable<T>> Get(params TKey[] keys)
        {
            await this.EnsureLoaded();
            return
                keys.Select(k => this._cache.Get<T>(this._source.Prefix + this._source.ToKey(k))).Where(t => t != null);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            await this.EnsureLoaded();
            return this._cache.GetRegion<T>(this._source.Prefix, i => this._source.Prefix + this._source.ToKey(i));
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
                        this._cache.Put(this._source.Prefix + this._source.ToKey(v.Value), v.Value);
                        break;
                    case CrudAction.Delete:
                        this._cache.Remove(this._source.Prefix + this._source.ToKey(v.Value));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public virtual string StoreIdentifier { get; }

        private async Task EnsureLoaded()
        {
            if (this._isLoaded)
                return;

            await this._elector.ExecuteIfLeader(ApplicationNode.Identifier, this.StoreIdentifier, async () =>
            {
                // If we are leader, then we try to load data from source
                IEnumerable<T> values = await this._loaderAsyncLazy;
                KeyValuePair<CacheKey, T>[] keyValues =
                    values.Select(i => new KeyValuePair<CacheKey, T>(this._source.Prefix + this._source.ToKey(i), i))
                        .ToArray();
                this._cache.PutRegion(keyValues);
                this._isLoaded = true;
            });

            if (this._isLoaded)
                return;

        }
    }
}