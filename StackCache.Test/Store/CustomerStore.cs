namespace StackCache.Test.Store
{
    using Core;
    using Core.CacheKeys;
    using Core.Election;
    using Core.Stores;
    using Data;

    public class CustomerStore : Store<CustomerDomain, int>
    {
        public CustomerStore(ICache cache, IElection elector)
            : base(cache, () => new CustomerSource(), elector)
        {
        }

        protected override Key ToKey(int key)
        {
            return key.ToString();
        }

        protected override Key ToKey(CustomerDomain value)
        {
            return value.Id.ToString();
        }
    }
}