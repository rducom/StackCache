namespace StackCache.Core.CacheKeys
{
    using System.Runtime.Serialization;
    using ProtoBuf;

    /// <summary>
    /// A key prefix describe the first part of a CacheKey. It's reusable.
    /// </summary>
    [ProtoContract]
    public struct KeyPrefix
    {
        public KeyPrefix(Key tenant, Key region)
            : this()
        {
            this._tenant = tenant;
            this._region = region;
        }

        //public KeyPrefix(SerializationInfo info, StreamingContext text)
        //    : this()
        //{
        //    this._tenant = (Key)info.GetValue(nameof(this.Tenant), typeof(Key));
        //    this._region = (Key)info.GetValue(nameof(this.Region), typeof(Key));
        //}

        public Key Tenant => this._tenant;
        public Key Region => this._region;

        public string SearchPattern => new CacheKey(this, "*");

        public static KeyPrefix Null = new KeyPrefix(Key.Null, Key.Null);

        [ProtoMember(1)]
        private readonly Key _tenant;

        [ProtoMember(2)]
        private readonly Key _region;

        public static CacheKey operator +(KeyPrefix x, Key y)
        {
            return new CacheKey(x, y);
        }

        public static bool operator !=(KeyPrefix x, KeyPrefix y)
        {
            return !(x == y);
        }

        public static bool operator ==(KeyPrefix x, KeyPrefix y)
        {
            return x.Tenant == y.Tenant && x.Region == y.Region;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is KeyPrefix))
                return false;
            var other = (KeyPrefix)obj;
            return this == other;
        }

        //public override int GetHashCode()
        //{
        //    int hash1 = this.Tenant.IsNullOrEmpty ? 0 : this.Tenant.GetHashCode();
        //    int hash2 = this.Region.IsNullOrEmpty ? 0 : this.Region.GetHashCode();
        //    return unchecked((31*hash1) + hash2);
        //}

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17 * 29 + this.Tenant.GetHashCode();
                return hash * 29 + this.Region.GetHashCode();
            }
        }

        public override string ToString()
        {
            return Key.Separator + this.Tenant + Key.Separator + this.Region;
        }
    }
}