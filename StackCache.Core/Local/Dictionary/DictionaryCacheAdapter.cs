namespace StackCache.Core.Local.Dictionary
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CacheKeys;
    using CacheValues;
    using Locking;

    public class DictionaryCacheAdapter : ICacheAdapter
    {
        private readonly ConcurrentDictionary<KeyPrefix, ConcurrentDictionary<Key, ICacheValue>> _dic = new ConcurrentDictionary<KeyPrefix, ConcurrentDictionary<Key, ICacheValue>>();

        private ConcurrentDictionary<Key, ICacheValue> Sub(KeyPrefix prefix)
        {
            return this._dic.GetOrAdd(prefix, p => new ConcurrentDictionary<Key, ICacheValue>());
        }

        public CacheType CacheType => CacheType.Local;

        public bool Get<T>(CacheKey key, out T value)
        {
            ICacheValue searched;
            if (this.Sub(key.Prefix).TryGetValue(key.Suffix, out searched))
            {
                var found = (CacheValue<T>)searched;
                if (found.IsInvalidated == false)
                {
                    value = found.Value;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        public void Put<T>(CacheKey key, T value)
        {
            this.Sub(key.Prefix).AddOrUpdate(key.Suffix, (CacheValue<T>)value, (k, val) => (CacheValue<T>)value);
        }

        public T GetOrCreate<T>(CacheKey key, Func<CacheKey, T> cacheValueCreator)
        {
            if (cacheValueCreator == null) throw new ArgumentNullException(nameof(cacheValueCreator));
            return (CacheValue<T>)this.Sub(key.Prefix).GetOrAdd(key.Suffix, (CacheValue<T>)cacheValueCreator(key));
        }

        public void Remove(CacheKey key)
        {
            ICacheValue searched;
            this.Sub(key.Prefix).TryRemove(key.Suffix, out searched);
        }

        public IEnumerable<T> GetRegion<T>(KeyPrefix prefix)
        {
            return this.Sub(prefix).Values.OfType<CacheValue<T>>().Where(i => i.IsInvalidated == false).Select(i => i.Value);
        }

        public Task<IEnumerable<T>> GetRegionAsync<T>(KeyPrefix prefix)
        {
            var result = this.Sub(prefix).Values.OfType<CacheValue<T>>().Where(i => i.IsInvalidated == false).Select(i => i.Value);
            return Task.FromResult(result);
        }

        public Task<IEnumerable<KeyValuePair<CacheKey, T>>> GetRegionKeyValuesAsync<T>(KeyPrefix prefix)
        {
            List<KeyValuePair<CacheKey, T>> result = this.Sub(prefix)
                .Where(i => i.Value.IsInvalidated == false)
                .Select(kv => new KeyValuePair<CacheKey, T>(prefix + kv.Key, ((CacheValue<T>)kv.Value).Value))
                .ToList();
            return Task.FromResult(result.AsEnumerable());
        }
        public void RemoveRegion(KeyPrefix prefix)
        {
            ConcurrentDictionary<Key, ICacheValue> toremove;
            this._dic.TryRemove(prefix, out toremove);
        }

        public ILock GetLocker()
        {
            return new LocalLock();
        }

        public void PutRegion<T>(KeyValuePair<CacheKey, T>[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            foreach (KeyValuePair<CacheKey, T> kv in values)
            {
                this.Sub(kv.Key.Prefix).AddOrUpdate(kv.Key.Suffix, (CacheValue<T>)kv.Value, (k, val) => (CacheValue<T>)kv.Value);
            }
        }

        public void Invalidate(params CacheKey[] keys)
        {
            foreach (CacheKey key in keys)
            {
                ICacheValue value;
                if (this.Sub(key.Prefix).TryGetValue(key.Suffix, out value))
                {
                    value.IsInvalidated = true;
                }
            }
        }


    }
}