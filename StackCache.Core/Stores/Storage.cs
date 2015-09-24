namespace StackCache.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CacheKeys;

    /// <summary>
    /// Provide an ensemblist storage for a Type/Region
    /// Such Storage contains all the referenced instances
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class Storage<T, TKey> : IStorage<T, TKey>
        where T : class
    {
        private readonly ICache _cache;
        private readonly IDatabaseSourceGlobal<T, TKey> _source;

        protected Storage(ICache cache, IDatabaseSourceGlobal<T, TKey> source)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (source == null) throw new ArgumentNullException(nameof(source));
            Debug.Assert(cache != null, "cache != null");
            Debug.Assert(source != null, "cache != null");
            this._cache = cache;
            this._source = source;
        }

        public async Task<T> Get(TKey key)
        {
            await this.EnsureLoaded();
            return this._cache.Get<T>(this._source.Prefix + this._source.ToKey(key));
        }

        public async Task<IEnumerable<T>> Get(IEnumerable<TKey> keys)
        {
            await this.EnsureLoaded();
            return keys.Select(k => this._cache.Get<T>(this._source.Prefix + this._source.ToKey((TKey)k))).Where(t => t != null);
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

        protected virtual TimeSpan LoadingLockTimeout => TimeSpan.FromMinutes(1);

        private async Task EnsureLoaded()
        {
            var keyLoading = new CacheKey(this._source.Prefix, "LoadingInfo");
            var keyLoaded = new CacheKey(this._source.Prefix, "LoadedInfo");
            var keyLock = new CacheKey(this._source.Prefix, "LoadLock");

            var infoLoaded = this._cache.Get<LoadedInfo>(keyLoaded);

            if (infoLoaded != null)
            {
            }
            else
            {
                var tcs = new CancellationTokenSource();
                using (await this._cache.Lock(keyLock, this.LoadingLockTimeout, tcs.Token))
                {
                    var infoLoading = this._cache.Get<LoadingInfo>(keyLoading);
                    if (infoLoading == null)
                    {
                        infoLoading = new LoadingInfo
                        {
                            Prefix = this._source.Prefix,
                            LoadStart = DateTime.UtcNow,
                            MachineName = Environment.MachineName,
                        };
                        this._cache.Put(keyLoading, infoLoading);
                    }

                    IEnumerable<T> values = await this._source.Load();

                    KeyValuePair<CacheKey, T>[] keyValues = values.Select(i => new KeyValuePair<CacheKey, T>(this._source.Prefix + this._source.ToKey(i), i)).ToArray();

                    this._cache.Put(keyLoaded, new LoadedInfo(infoLoading, DateTime.UtcNow, keyValues.Count()));
                    this._cache.PutRegion(keyValues);
                }
            }
        }


    }
}