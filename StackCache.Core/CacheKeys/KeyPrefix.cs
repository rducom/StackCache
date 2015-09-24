namespace StackCache.Core.CacheKeys
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            this.Tenant = tenant;
            this.Subset = region;
        }

        [ProtoMember(1)]
        public Key Tenant { get; private set; }

        [ProtoMember(2)]
        public Key Subset { get; private set; }

        public static implicit operator string(KeyPrefix key)
        {
            return key.Tenant + Key.Separator + key.Subset;
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
            throw new Exception("No more 1 '|' separator allowed");
        }

        public string SearchPattern => this + Key.Separator + "*";

        public static KeyPrefix Null = new KeyPrefix(null, null);


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
            return x.Tenant == y.Tenant && x.Subset == y.Subset;
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
            int hash2 = this.Subset.IsNullOrEmpty ? 0 : this.Subset.GetHashCode();
            return unchecked((31 * hash1) + hash2);
        }

    }
}