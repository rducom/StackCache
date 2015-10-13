namespace StackCache.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CacheKeys;
    using Helpers;
    using Local.Dictionary;
    using Locking;
    using Messaging;

    public class Cache : ICache
    {
        private static volatile Cache _defaultCache;
        private static readonly object _defaultCacheInitLock = new object();
        private readonly string _genericChannel = ".";
        private readonly string _identifier = Guid.NewGuid().ToString();
        private readonly IMessenger _messenger;

        public Cache(ILocalCacheAdapter memoryCache, IDistributedCacheAdapter distributedCache, IMessenger messenger)
        {
            this.DistributedCache = distributedCache;
            this.LocalCache = memoryCache ?? new DictionaryCacheAdapter();
            this._messenger = messenger;
            this._messenger?.Subscribe<DataNotification>(this._genericChannel, this.OnNotification);
        }

        public static Cache Default
        {
            get
            {
                if (_defaultCache != null)
                    return _defaultCache;
                lock (_defaultCacheInitLock)
                {
                    if (_defaultCache == null)
                        _defaultCache = new Cache(null, null, null);
                }
                return _defaultCache;
            }
        }

        public IDistributedCacheAdapter DistributedCache { get; }

        public ILocalCacheAdapter LocalCache { get; }

        public T Get<T>(CacheKey key)
        {
            T value;
            if (this.LocalCache.Get(key, out value))
                return value;
            if (this.DistributedCache == null || !this.DistributedCache.Get(key, out value))
                return default(T);
            this.LocalCache.Put(key, value);
            return value;
        }

        public void Put<T>(CacheKey key, T value)
        {
            this.LocalCache?.Put(key, value);
            this.DistributedCache?.Put(key, value);
            this._messenger?.Notify(this._genericChannel,
                new DataNotification(this._identifier, NotificationType.UpdatedItem, new[] { key }));
        }

        public void Remove(CacheKey key)
        {
            this.LocalCache?.Remove(key);
            this.DistributedCache?.Remove(key);
            this._messenger?.Notify(this._genericChannel, new DataNotification(this._identifier, NotificationType.RemovedItem, key));
        }

        public void RemoveRegion(KeyPrefix prefix)
        {
            this.LocalCache?.RemoveRegion(prefix);
            this.DistributedCache?.RemoveRegion(prefix);
            this._messenger?.Notify(this._genericChannel, new DataNotification(this._identifier, NotificationType.RemovedRegion, new CacheKey(prefix, Key.Null)));
        }
         
        public Key Tenant { get; }

        public IEnumerable<T> GetRegion<T>(KeyPrefix prefix, Func<T, CacheKey> keyFromValue)
        {
            if (keyFromValue == null) throw new ArgumentNullException(nameof(keyFromValue));

            IEnumerable<T> values = this.LocalCache.GetRegion<T>(prefix);
            if (values != null && values.Any())
                return values;
            if (this.DistributedCache != null)
                values = this.DistributedCache.GetRegion<T>(prefix);
            if (values == null || !values.Any())
                return Enumerable.Empty<T>();
            KeyValuePair<CacheKey, T>[] dic =
                values.Select(i => new KeyValuePair<CacheKey, T>(keyFromValue(i), i)).ToArray();
            this.LocalCache.PutRegion(dic);
            return values;
        }

        public async Task<IEnumerable<T>> GetRegionAsync<T>(KeyPrefix prefix, Func<T, CacheKey> keyFromValue)
        {
            if (keyFromValue == null) throw new ArgumentNullException(nameof(keyFromValue));
            IEnumerable<T> values = await this.LocalCache.GetRegionAsync<T>(prefix);
            if (values != null && values.Any())
                return values;
            if (this.DistributedCache != null)
                values = await this.DistributedCache.GetRegionAsync<T>(prefix);
            if (values == null || !values.Any())
                return Enumerable.Empty<T>();
            KeyValuePair<CacheKey, T>[] dic =
                values.Select(i => new KeyValuePair<CacheKey, T>(keyFromValue(i), i)).ToArray();
            this.LocalCache.PutRegion(dic);
            return values;
        }

        public async Task<IEnumerable<KeyValuePair<CacheKey, T>>> GetRegionKeyValuesAsync<T>(KeyPrefix prefix)
        {
            IEnumerable<KeyValuePair<CacheKey, T>> values = await this.LocalCache.GetRegionKeyValuesAsync<T>(prefix);
            if (values != null && values.Any())
                return values;
            if (this.DistributedCache != null)
                values = await this.DistributedCache.GetRegionKeyValuesAsync<T>(prefix);
            if (values == null || !values.Any())
                return Enumerable.Empty<KeyValuePair<CacheKey, T>>();
            this.LocalCache.PutRegion(values.ToArray());
            return values;
        }

        public T GetOrCreate<T>(CacheKey key, Func<CacheKey, T> cacheValueCreator)
        {
            T value;
            if (this.LocalCache.Get(key, out value))
            {
                return value;
            }
            if (this.DistributedCache != null && this.DistributedCache.Get(key, out value))
            {
                this.LocalCache.Put(key, value);
                return value;
            }
            value = cacheValueCreator(key);
            this.LocalCache.Put(key, value);
            this.DistributedCache?.Put(key, value);
            this._messenger?.Notify(this._genericChannel, new DataNotification(this._identifier, NotificationType.UpdatedItem, new[] { key }));
            return value;
        }

        public void PutRegion<T>(KeyValuePair<CacheKey, T>[] values)
        {
            this.LocalCache.PutRegion(values);
            this.DistributedCache?.PutRegion(values);
            this._messenger?.Notify(this._genericChannel, new DataNotification(this._identifier, NotificationType.UpdatedRegion, values.Select(i => i.Key).ToArray()));
        }

        private void OnNotification(string channel, DataNotification dn)
        {
            if (dn == null || dn.Source == this._identifier)
                return;
            switch (dn.NotificationType)
            {
                case NotificationType.UpdatedItem:
                    this.LocalCache.Invalidate(dn.Keys);
                    break;
                case NotificationType.UpdatedRegion:
                    this.LocalCache.Invalidate(dn.Keys);
                    break;
                case NotificationType.RemovedItem:
                    foreach (CacheKey key in dn.Keys)
                        this.LocalCache.Remove(key);
                    break;
                case NotificationType.RemovedRegion:
                    foreach (CacheKey key in dn.Keys)
                        this.LocalCache.Remove(key);
                    break;
            }
        }
    }
}