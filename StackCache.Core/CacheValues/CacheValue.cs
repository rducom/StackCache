namespace StackCache.Core.CacheValues
{
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

        public bool IsInvalidated { get; set; }
    }
}