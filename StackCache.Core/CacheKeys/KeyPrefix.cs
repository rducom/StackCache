namespace StackCache.Core.CacheKeys
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using ProtoBuf;

    /// <summary>
    /// A key prefix describe the first part of a CacheKey. It's reusable.
    /// </summary>
    [ProtoContract]
    public struct KeyPrefix
    {
        public KeyPrefix(string tenant, string region)
            : this()
        {
            this._tenant = tenant;
            this._region = region;
        }

        public KeyPrefix(SerializationInfo info, StreamingContext text)
                : this()
        {
            this._tenant = (Key)info.GetValue(nameof(this.Tenant), typeof(Key));
            this._region = (Key)info.GetValue(nameof(this.Region), typeof(Key));
        }

        public Key Tenant => this._tenant;
        public Key Region => this._region;

        public static implicit operator string(KeyPrefix key)
        {
            return key.Tenant + Key.Separator + key.Region;
        }

        public static implicit operator KeyPrefix(string key)
        {
            if (key == null)
                return Null;
            List<string> splitted = key.Split(Key.Separator).ToList();
            switch (splitted.Count)
            {
                case 0:
                    return Null;
                case 1:
                    return new KeyPrefix(null, splitted[0]);
                case 2:
                    return new KeyPrefix(splitted[0], splitted[1]);
            }
            return KeyPrefix.Null;
        }

        public string SearchPattern => this + Key.Separator + "*";

        public static KeyPrefix Null = new KeyPrefix(null, null);

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

        public override int GetHashCode()
        {
            int hash1 = this.Tenant.IsNullOrEmpty ? 0 : this.Tenant.GetHashCode();
            int hash2 = this.Region.IsNullOrEmpty ? 0 : this.Region.GetHashCode();
            return unchecked((31 * hash1) + hash2);
        }

    }
}