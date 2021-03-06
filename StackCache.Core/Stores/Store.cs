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
    public abstract class Store<T, TKey> : StoreBase, IStore<T, TKey>
        where T : class
    {
        private readonly ICache _cache;

        private readonly AsyncLazy<bool> _loaderAsyncLazy;
        private readonly Func<IDatabaseSourceGlobal<T, TKey>> _source;
        private readonly IElection _elector;
        private bool _isLoaded;

        protected Store(ICache cache, Func<IDatabaseSourceGlobal<T, TKey>> source, IElection elector)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (source == null) throw new ArgumentNullException(nameof(source));
            Debug.Assert(cache != null, "cache != null");
            Debug.Assert(source != null, "cache != null");
            this._cache = cache;
            this._source = source;
            this._elector = elector;
            this._loaderAsyncLazy = new AsyncLazy<bool>(this.EnsureStoreReady);
        }

        protected override Type StoreType => typeof(T);

        protected override Key Tenant => this._cache.Tenant;

        public async Task<T> Get(TKey key)
        {
            await this.EnsureReady();
            return this._cache.Get<T>(this.Prefix + this.ToKey(key));
        }

        public async Task<IEnumerable<T>> Get(params TKey[] keys)
        {
            await this.EnsureReady();
            return
                keys.Select(k => this._cache.Get<T>(this.Prefix + this.ToKey(k))).Where(t => t != null);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            await this.EnsureReady();
            return this._cache.GetRegion<T>(this.Prefix, i => this.Prefix + this.ToKey(i));
        }

        public async Task Save(IEnumerable<Crud<T>> value)
        {
            await this.EnsureReady();
            IEnumerable<Crud<T>> enumerable = value as IList<Crud<T>> ?? value.ToList();
            IDatabaseSourceGlobal<T, TKey> source = this._source();
            await source.Save(enumerable);
            foreach (Crud<T> v in enumerable)
            {
                switch (v.Action)
                {
                    case CrudAction.Insert:
                    case CrudAction.Update:
                        this._cache.Put(this.Prefix + this.ToKey(v.Value), v.Value);
                        break;
                    case CrudAction.Delete:
                        this._cache.Remove(this.Prefix + this.ToKey(v.Value));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected abstract Key ToKey(TKey key);

        protected abstract Key ToKey(T value);

        private async Task EnsureReady()
        {
            await this._loaderAsyncLazy;
        }

        private async Task<bool> EnsureStoreReady()
        {
            bool isLeader = await this.EnsureInitialized();
            if (isLeader == false)
            {
                // TODO initialization fallback :
                // TODO : if not leader, then check if other nodes exists, are leader, and are still working (no timeout, app blocking etc)
                // if no, then be the leader ?
                // if yes, then await for the next RegionChange notification, and return only when data is in L1
            }
            return true;
        }

        private async Task<bool> EnsureInitialized()
        {
            if (this._isLoaded)
                return false;

            return await this._elector.ExecuteIfLeader(ApplicationNode.Identifier, this.StoreIdentifier, async (token) =>
            {
                IDatabaseSourceGlobal<T, TKey> sourceInstance = this._source();
                // If we are leader, then we try to load data from source
                IEnumerable<T> values = await sourceInstance.Load();
                KeyValuePair<CacheKey, T>[] keyValues =
                    values.Select(i => new KeyValuePair<CacheKey, T>(this.Prefix + this.ToKey(i), i)).ToArray();
                this._cache.PutRegion(keyValues);
                this._isLoaded = true;
                return true;
            });
        }
    }
}