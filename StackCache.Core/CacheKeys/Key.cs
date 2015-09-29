namespace StackCache.Core.CacheKeys
{
    using System.Runtime.Serialization;
    using System.Text;
    using ProtoBuf;

    /// <summary>
    /// A Key is a chunk of key description. It can be used to define tenant, region, or identity part of a CacheKey
    /// </summary>
    [ProtoContract]
    public struct Key
    {
        [ProtoMember(1)]
        private readonly byte[] _key;
        internal const char Separator = '|';

        private Key(byte[] keyData)
        {
            this._key = keyData;
        }

        public Key(SerializationInfo info, StreamingContext text)
            : this()
        {
            this._key = (byte[])info.GetValue(nameof(this._key), typeof(Key));
        }

        public bool IsNullOrEmpty => this._key == null || this._key.Length == 0;

        /// <summary>
        /// Indicate whether two keys are not equal
        /// </summary>
        public static bool operator !=(Key x, Key y)
        {
            return !KeyHelper.Equals(x._key, y._key);
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
            return KeyHelper.Equals(this._key, other._key);
        }

        public override int GetHashCode()
        {
            return KeyHelper.GetHashCode(this._key);
        }

        public static Key Null = new Key(null);

        public static implicit operator Key(string key)
        {
            return string.IsNullOrEmpty(key) ? Null : new Key(Encoding.UTF8.GetBytes(key));
        }

        public static implicit operator Key(byte[] key)
        {
            return new Key(key);
        }

        public static implicit operator string (Key key)
        {
            return key.ToString();
        }

        public override string ToString()
        {
            return this._key == null ? string.Empty : Encoding.UTF8.GetString(this._key);
        }

        public static implicit operator byte[] (Key key)
        {
            return key._key;
        }
    }
}