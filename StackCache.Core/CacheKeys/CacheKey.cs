namespace StackCache.Core.CacheKeys
{
    using System;
    using System.Runtime.Serialization;
    using ProtoBuf;
    using StackExchange.Redis;

    /// <summary>
    /// CacheKey is a key defined by a prefix and an identity key
    /// Prefix intends to optimize cache regions and can be Null
    /// Suffix intends to identify a single item and must be unique in a region
    /// </summary>
    [ProtoContract]
    public struct CacheKey
    {
        public CacheKey(KeyPrefix prefix, Key suffix)
            : this()
        {
            this._prefix = prefix;
            this._suffix = suffix;
            this.ExpirationMode = ExpirationMode.None;
        }

        public CacheKey(Key tenant, Key region, Key suffix)
            : this()
        {
            this._prefix = new KeyPrefix(tenant, region);
            this._suffix = suffix;
            this.ExpirationMode = ExpirationMode.None;
        }

        // this constructor is used for deserialization
        public CacheKey(SerializationInfo info, StreamingContext text)
            : this()
        {
            this._prefix = (KeyPrefix) info.GetValue(nameof(this.Prefix), typeof (KeyPrefix));
            this._suffix = (Key) info.GetValue(nameof(this.Suffix), typeof (Key));
        }

        private static readonly CacheKey _null = new CacheKey(Key.Null, Key.Null, Key.Null);

        [ProtoMember(1)] private readonly KeyPrefix _prefix;

        [ProtoMember(2)] private readonly Key _suffix;

        /// <summary>
        /// CacheKey Prefix identifies Tenant + Region
        /// </summary>
        public KeyPrefix Prefix => this._prefix;

        /// <summary>
        /// CacheKey Suffix identifies a single item in a KeyPrefix region
        /// </summary>
        public Key Suffix => this._suffix;

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
                int hash = 17;
                hash = hash*29 + this.Prefix.GetHashCode();
                hash = hash*29 + this.Suffix.GetHashCode();
                return hash;
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
            return Key.Separator + this.Prefix.Tenant + Key.Separator + this.Prefix.Region + Key.Separator + this.Suffix;
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
            return key.ToString();
        }

        public static implicit operator CacheKey(string key)
        {
            if (key == null)
                return _null;
            string[] splitted = key.Split(Key.Separator);
            switch (splitted.Length)
            {
                case 1:
                    return new CacheKey(Key.Null, Key.Null, splitted[0]);
                case 2:
                    return new CacheKey(Key.Null, splitted[0], splitted[1]);
                case 3:
                    return new CacheKey(splitted[0], splitted[1], splitted[2]);
                case 4:
                    return new CacheKey(splitted[1], splitted[2], splitted[3]);
                default:
                    return _null;
            }
        }
    }
}