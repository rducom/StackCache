namespace StackCache.Core.CacheKeys
{
    using System.Text;
    using ProtoBuf;

    /// <summary>
    /// A Key is a chunk of key description. It can be used to define tenant, region, or identity part of a CacheKey
    /// </summary>
    [ProtoContract]
    public struct Key
    {
        private readonly byte[] _key;
        internal const char Separator = '|';

        public Key(byte[] keyData)
        {
            this._key = keyData;
        }

        internal bool IsNullOrEmpty => this._key == null || this._key.Length == 0;

        public byte[] KeyData => this._key;

        /// <summary>
        /// Indicate whether two keys are not equal
        /// </summary>
        public static bool operator !=(Key x, Key y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two keys are equal
        /// </summary>
        public static bool operator ==(Key x, Key y)
        {
            return KeyHelper.Equals(x._key, y._key);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Key))
                return false;
            var other = (Key)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            return this._key == null ? 0 : KeyHelper.GetHashCode(this._key);
        }

        public static Key Null = new Key(null);

        public static implicit operator Key(string key)
        {
            return key == null ? Null : new Key(Encoding.UTF8.GetBytes(key));
        }

        public static implicit operator string(Key key)
        {
            return key == null ? null : Encoding.UTF8.GetString(key._key);
        }
    }
}