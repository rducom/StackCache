namespace StackCache.Test.Fixture
{
    using System;
    using System.Data.Entity;
    using Data;
    using Core;
    using Core.Election;

    public abstract class CacheFixture : IDisposable
    {
        protected CacheFixture()
        {
            Database.SetInitializer(new CustomersTestData());
        }

        public virtual void Dispose()
        {
        }

        public ICache Cache { get; protected set; }

        public IElection Elector { get; set; }
    }
}