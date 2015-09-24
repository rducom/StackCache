namespace Caching.Test.Fixture
{
    using System;
    using System.Data.Entity;
    using Data;
    using StackCache.Core;

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
    }
}