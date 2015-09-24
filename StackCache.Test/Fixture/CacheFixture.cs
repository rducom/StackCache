using System;
using System.Data.Entity;

namespace Caching.Test.Fixture
{
    using Data;
    using StackCache.Core;

    public abstract class CacheFixture : IDisposable
    {
        protected CacheFixture()
        {
            Database.SetInitializer(new CustomersTestData());
        }

        public void Dispose()
        {
        }

        public ICache Cache { get; protected set; }
    }
}