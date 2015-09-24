namespace StackCache.Core.CacheKeys
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;
    using StackExchange.Redis;

    /// <summary>
    /// CacheKey is a key defined by a prefix and a identity key
    /// Prefix intends to optimize cache regions and can be null
    /// Key intends to identity a single item and must be unique in a region
    /// </summary>
    [ProtoContract]
    public struct CacheKey
    {
        private CacheKey(string prefix, string region, string key)
            : this()
        {
            this.Prefix = new KeyPrefix(prefix, region);
            this.Suffix = key;
            this.ExpirationMode = ExpirationMode.None;
        }

        public CacheKey(KeyPrefix prefix, Key sufffix)
            : this()
        {
            this.Prefix = prefix;
            this.Suffix = sufffix;
            this.ExpirationMode = ExpirationMode.None;
        }

        private static readonly CacheKey _null = new CacheKey(null, null, null);

        public KeyPrefix Prefix { get; }
        public Key Suffix { get; }

        public ExpirationMode ExpirationMode { get; private set; }
        public DateTime? ExpirationDateTime { get; private set; }
        public TimeSpan? ExpirationTimeSpan { get; private set; }

        private bool Equals(CacheKey other)
        {
            return this.Prefix.Equals(other.Prefix) && this.Suffix.Equals(other.Suffix);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CacheKey && this.Equals((CacheKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Prefix.GetHashCode()*397) ^ this.Suffix.GetHashCode();
            }
        }

        public void SetExpiration(TimeSpan timeSpan)
        {
            this.ExpirationTimeSpan = timeSpan;
            this.ExpirationMode = ExpirationMode.Sliding;
        }

        public void SetExpiration(DateTime dateTime)
        {
            this.ExpirationDateTime = dateTime;
            this.ExpirationMode = ExpirationMode.Sliding;
        }

        /// <summary>
        /// Indicate whether two keys are not equal
        /// </summary>
        public static bool operator !=(CacheKey x, CacheKey y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two keys are equal
        /// </summary>
        public static bool operator ==(CacheKey x, CacheKey y)
        {
            return x.Prefix == y.Prefix && x.Suffix == y.Suffix;
        }

        public override string ToString()
        {
            return this;
        }

        public static implicit operator RedisKey(CacheKey key)
        {
            return (string) key;
        }

        public static implicit operator CacheKey(RedisKey key)
        {
            return (string) key;
        }

        public static implicit operator string(CacheKey key)
        {
            return key.Prefix + Key.Separator + key.Suffix;
        }

        public static implicit operator CacheKey(string key)
        {
            if (key == null)
                return _null;
            List<string> splitted = key.Split(Key.Separator).ToList();
            switch (splitted.Count)
            {
                case 0:
                    return _null;
                case 1:
                    return new CacheKey(null, null, splitted[0]);
                case 2:
                    return new CacheKey(null, splitted[0], splitted[1]);
                case 3:
                    return new CacheKey(splitted[0], splitted[1], splitted[2]);
            }
            throw new Exception("No more 2 '|' separators allowed");
        }
    }
}