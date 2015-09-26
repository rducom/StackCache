namespace StackCache.Test.Store
{
    using Data;
    using Core;
    using Core.Election;
    using Core.Stores;

    public class CustomerStore : Store<CustomerDomain, int>
    {

        public CustomerStore(ICache cache, IElection elector)
            : base(cache, new CustomerSource(), elector)
        {
        }
    }


}