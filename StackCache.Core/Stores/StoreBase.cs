namespace StackCache.Core.Stores
{
    using System;
    using CacheKeys;

    public abstract class StoreBase
    {
        protected virtual string StoreIdentifier => this.StoreType.Name;
        protected abstract Type StoreType { get; }
        protected abstract Key Tenant { get; }
        private KeyPrefix _prefix;

        protected KeyPrefix Prefix
        {
            get
            {
                if (this._prefix != KeyPrefix.Null)
                    return this._prefix;
                this._prefix = new KeyPrefix(this.Tenant, this.StoreIdentifier);
                return this._prefix;
            }
        }
    }
}