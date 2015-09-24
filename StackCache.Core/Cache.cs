namespace StackCache.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CacheKeys;
    using Local.Dictionary;
    using Locking;
    using Messaging;

    public class Cache : ICache
    {
        private readonly ICacheAdapter _distributedCache;
        private readonly ICacheAdapter _memoryCache;
        private readonly IMessenger _messenger;
        private readonly string _identitifer = Guid.NewGuid().ToString();
        private readonly string _genericChannel = ".";

        public Cache(ICacheAdapter memoryCache, ICacheAdapter distributedCache, IMessenger messenger)
        {
            this._distributedCache = distributedCache;
            this._memoryCache = memoryCache ?? new DictionaryCacheAdapter();
            this._messenger = messenger;
            this._messenger?.Subscribe(this._genericChannel, this.OnNotification);
        }

        private void OnNotification(string channel, Notification notification)
        {
            if (notification == null || notification.Source == this._identitifer)
                return;

            switch (notification.Event)
            {
                case Event.UpdatedItem:
                    this._memoryCache.Invalidate(notification.Keys);
                    break;
                case Event.UpdatedRegion:
                    this._memoryCache.Invalidate(notification.Keys);
                    break;
                case Event.RemovedItem:
                    foreach (CacheKey key in notification.Keys)
                        this._memoryCache.Remove(key);
                    break;
                case Event.RemovedRegion:
                    foreach (CacheKey key in notification.Keys)
                        this._memoryCache.Remove(key);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static volatile Cache _defaultCache;
        private static readonly object _defaultCacheInitLock = new object();

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

        public T Get<T>(CacheKey key)
        {
            T value;
            if (this._memoryCache.Get(key, out value))
                return value;
            if (this._distributedCache == null || !this._distributedCache.Get(key, out value))
                return default(T);
            this._memoryCache.Put(key, value);
            return value;
        }

        public void Put<T>(CacheKey key, T value)
        {
            this._memoryCache?.Put(key, value);
            this._distributedCache?.Put(key, value);
            this._messenger?.Notify(this._genericChannel, new Notification(this._identitifer, Event.UpdatedItem, key));
        }

        public void Remove(CacheKey key)
        {
            this._memoryCache?.Remove(key);
            this._distributedCache?.Remove(key);
        }

        public void RemoveRegion(KeyPrefix prefix)
        {
            this._memoryCache?.RemoveRegion(prefix);
            this._distributedCache?.RemoveRegion(prefix);
        }

        public async Task<IDisposable> Lock(string key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            ILock local = this._memoryCache.GetLocker();
            IDisposable localDispose = await local.Lock(key, timeout, cancellationToken);

            if (this._distributedCache == null)
                return localDispose;

            ILock distributed = this._distributedCache.GetLocker();
            IDisposable remoteDispose = await distributed.Lock(key, timeout, cancellationToken);

            return new WrapDisposable(localDispose, remoteDispose);
        }

        public IEnumerable<T> GetRegion<T>(KeyPrefix prefix, Func<T, CacheKey> keyFromValue)
        {
            if (keyFromValue == null) throw new ArgumentNullException(nameof(keyFromValue));

            IEnumerable<T> values = this._memoryCache.GetRegion<T>(prefix);
            if (values != null)
                return values;
            if (this._distributedCache != null)
                values = this._distributedCache.GetRegion<T>(prefix);
            if (values == null)
                return null;
            KeyValuePair<CacheKey, T>[] dic = values.Select(i => new KeyValuePair<CacheKey, T>(keyFromValue(i), i)).ToArray();
            this._memoryCache.PutRegion(dic);
            return values;
        }

        public async Task<IEnumerable<T>> GetRegionAsync<T>(KeyPrefix prefix, Func<T, CacheKey> keyFromValue)
        {
            IEnumerable<T> values = await this._memoryCache.GetRegionAsync<T>(prefix);
            if (values != null)
                return values;
            if (this._distributedCache != null)
                values = await this._distributedCache.GetRegionAsync<T>(prefix);
            if (values == null)
                return null;
            KeyValuePair<CacheKey, T>[] dic = values.Select(i => new KeyValuePair<CacheKey, T>(keyFromValue(i), i)).ToArray();
            this._memoryCache.PutRegion(dic);
            return values;
        }

        public async Task<IEnumerable<KeyValuePair<CacheKey, T>>> GetRegionKeyValuesAsync<T>(KeyPrefix prefix)
        {
            IEnumerable<KeyValuePair<CacheKey, T>> values = await this._memoryCache.GetRegionKeyValuesAsync<T>(prefix);
            if (values != null)
                return values;
            if (this._distributedCache != null)
                values = await this._distributedCache.GetRegionKeyValuesAsync<T>(prefix);
            if (values == null)
                return null;
            this._memoryCache.PutRegion(values.ToArray());
            return values;
        }

        public T GetOrCreate<T>(CacheKey key, Func<CacheKey, T> cacheValueCreator)
        {
            T value;
            if (this._memoryCache.Get(key, out value))
            {
                return value;
            }
            if (this._distributedCache != null && this._distributedCache.Get(key, out value))
            {
                this._memoryCache.Put(key, value);
                return value;
            }
            value = cacheValueCreator(key);
            this._memoryCache.Put(key, value);
            this._distributedCache?.Put(key, value);
            this._messenger?.Notify(this._genericChannel, new Notification(this._identitifer, Event.UpdatedItem, key));
            return value;
        }

        public void PutRegion<T>(KeyValuePair<CacheKey, T>[] values)
        {
            this._memoryCache.PutRegion(values);
            this._distributedCache?.PutRegion(values);
            this._messenger?.Notify(this._genericChannel, new Notification(this._identitifer, Event.UpdatedRegion, values.Select(i => i.Key).ToArray()));
        }
    }
}