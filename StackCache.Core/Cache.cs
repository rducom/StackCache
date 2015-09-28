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

        public Cache(ICacheAdapter memoryCache, ICacheAdapter distributedCache, IMessenger messenger)
        {
            this.DistributedCache = distributedCache;
            this.MemoryCache = memoryCache ?? new DictionaryCacheAdapter();
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

        public ICacheAdapter DistributedCache { get; }

        public ICacheAdapter MemoryCache { get; }

        public T Get<T>(CacheKey key)
        {
            T value;
            if (this.MemoryCache.Get(key, out value))
                return value;
            if (this.DistributedCache == null || !this.DistributedCache.Get(key, out value))
                return default(T);
            this.MemoryCache.Put(key, value);
            return value;
        }

        public void Put<T>(CacheKey key, T value)
        {
            this.MemoryCache?.Put(key, value);
            this.DistributedCache?.Put(key, value);
            this._messenger?.Notify(this._genericChannel,
                new DataNotification(this._identifier, NotificationType.UpdatedItem, new[] {key}));
        }

        public void Remove(CacheKey key)
        {
            this.MemoryCache?.Remove(key);
            this.DistributedCache?.Remove(key);
        }

        public void RemoveRegion(KeyPrefix prefix)
        {
            this.MemoryCache?.RemoveRegion(prefix);
            this.DistributedCache?.RemoveRegion(prefix);
        }

        public async Task<IDisposable> Lock(string key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            ILock local = this.MemoryCache.GetLocker();
            IDisposable localDispose = await local.Lock(key, timeout, cancellationToken);

            if (this.DistributedCache == null)
                return localDispose;

            ILock distributed = this.DistributedCache.GetLocker();
            IDisposable remoteDispose = await distributed.Lock(key, timeout, cancellationToken);

            return new WrapDisposable(localDispose, remoteDispose);
        }

        public Key Tenant { get; }

        public IEnumerable<T> GetRegion<T>(KeyPrefix prefix, Func<T, CacheKey> keyFromValue)
        {
            if (keyFromValue == null) throw new ArgumentNullException(nameof(keyFromValue));

            IEnumerable<T> values = this.MemoryCache.GetRegion<T>(prefix);
            if (values != null)
                return values;
            if (this.DistributedCache != null)
                values = this.DistributedCache.GetRegion<T>(prefix);
            if (values == null)
                return null;
            KeyValuePair<CacheKey, T>[] dic =
                values.Select(i => new KeyValuePair<CacheKey, T>(keyFromValue(i), i)).ToArray();
            this.MemoryCache.PutRegion(dic);
            return values;
        }

        public async Task<IEnumerable<T>> GetRegionAsync<T>(KeyPrefix prefix, Func<T, CacheKey> keyFromValue)
        {
            IEnumerable<T> values = await this.MemoryCache.GetRegionAsync<T>(prefix);
            if (values != null)
                return values;
            if (this.DistributedCache != null)
                values = await this.DistributedCache.GetRegionAsync<T>(prefix);
            if (values == null)
                return null;
            KeyValuePair<CacheKey, T>[] dic =
                values.Select(i => new KeyValuePair<CacheKey, T>(keyFromValue(i), i)).ToArray();
            this.MemoryCache.PutRegion(dic);
            return values;
        }

        public async Task<IEnumerable<KeyValuePair<CacheKey, T>>> GetRegionKeyValuesAsync<T>(KeyPrefix prefix)
        {
            IEnumerable<KeyValuePair<CacheKey, T>> values = await this.MemoryCache.GetRegionKeyValuesAsync<T>(prefix);
            if (values != null)
                return values;
            if (this.DistributedCache != null)
                values = await this.DistributedCache.GetRegionKeyValuesAsync<T>(prefix);
            if (values == null)
                return null;
            this.MemoryCache.PutRegion(values.ToArray());
            return values;
        }

        public T GetOrCreate<T>(CacheKey key, Func<CacheKey, T> cacheValueCreator)
        {
            T value;
            if (this.MemoryCache.Get(key, out value))
            {
                return value;
            }
            if (this.DistributedCache != null && this.DistributedCache.Get(key, out value))
            {
                this.MemoryCache.Put(key, value);
                return value;
            }
            value = cacheValueCreator(key);
            this.MemoryCache.Put(key, value);
            this.DistributedCache?.Put(key, value);
            this._messenger?.Notify(this._genericChannel,
                new DataNotification(this._identifier, NotificationType.UpdatedItem, new[] {key}));
            return value;
        }

        public void PutRegion<T>(KeyValuePair<CacheKey, T>[] values)
        {
            this.MemoryCache.PutRegion(values);
            this.DistributedCache?.PutRegion(values);
            this._messenger?.Notify(this._genericChannel,
                new DataNotification(this._identifier, NotificationType.UpdatedRegion,
                    values.Select(i => i.Key).ToArray()));
        }

        private void OnNotification(string channel, DataNotification dn)
        {
            if (dn == null || dn.Source == this._identifier)
                return;
            switch (dn.NotificationType)
            {
                case NotificationType.UpdatedItem:
                    this.MemoryCache.Invalidate(dn.Keys);
                    break;
                case NotificationType.UpdatedRegion:
                    this.MemoryCache.Invalidate(dn.Keys);
                    break;
                case NotificationType.RemovedItem:
                    foreach (CacheKey key in dn.Keys)
                        this.MemoryCache.Remove(key);
                    break;
                case NotificationType.RemovedRegion:
                    foreach (CacheKey key in dn.Keys)
                        this.MemoryCache.Remove(key);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}