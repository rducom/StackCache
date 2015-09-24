namespace StackCache.Core.CacheValues
{
    /// <summary>
    /// Cache value container
    /// Enables invalidation mecanism in order to force local cache to retreive new value's data from 2nd or 3rd level cache
    /// </summary>
    /// <typeparam name="T">inner value Data type</typeparam>
    public class CacheValue<T> : ICacheValue
    {
        private CacheValue(T value)
        {
            this._value = value;
        }

        private readonly T _value;

        public T Value => this._value;

        public static implicit operator CacheValue<T>(T instance)
        {
            return new CacheValue<T>(instance);
        }

        public static implicit operator T(CacheValue<T> cacheValue)
        {
            return cacheValue != null ? cacheValue.Value : default(T);
        }

        /// <summary>
        /// True is the local data differs from remote data 
        /// Invalidation occurs after a value have been updated remotely
        /// </summary>
        public bool IsInvalidated { get; set; }
    }
}