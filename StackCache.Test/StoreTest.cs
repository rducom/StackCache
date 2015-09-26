namespace StackCache.Test
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Stores;
    using Data;
    using Fixture;
    using Store;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class StoreTest
    {
        private readonly CacheFixture _fix;
        private readonly ITestOutputHelper _output;

        protected StoreTest(CacheFixture fix, ITestOutputHelper output)
        {
            this._fix = fix;
            this._output = output;
            this.Store = new CustomerStore(fix.Cache, fix.Elector);
        }

        private CustomerStore Store { get; }

        [Fact]
        public async Task StoreGet()
        {
            var result = await this.Store.GetAll();
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task StoreSave_Create()
        {
            var created = new List<Crud<CustomerDomain>>
            {
                new Crud<CustomerDomain>(new CustomerDomain {Id = 42, Name = "Quarante deux"}, CrudAction.Insert)
            };
            await this.Store.Save(created);
            var found = await this.Store.Get(42);
            Assert.NotNull(found);
            Assert.Equal(found.Name, "Quarante deux");
        }

        [Fact]
        public async Task StoreSave_Update()
        {
            var created = new List<Crud<CustomerDomain>>
            {
                new Crud<CustomerDomain>(new CustomerDomain {Id = 42, Name = "Quarante deux 42"}, CrudAction.Update)
            };
            await this.Store.Save(created);
            var found = await this.Store.Get(42);
            Assert.NotNull(found);
            Assert.Equal(found.Name, "Quarante deux 42");
        }

        [Fact]
        public async Task StoreSave_Delete()
        {
            var created = new List<Crud<CustomerDomain>>
            {
                new Crud<CustomerDomain>(new CustomerDomain {Id = 42, Name = "this one should be deleted"},
                    CrudAction.Delete)
            };
            await this.Store.Save(created);
            var found = await this.Store.Get(42);
            Assert.Null(found);
        }
    }
}