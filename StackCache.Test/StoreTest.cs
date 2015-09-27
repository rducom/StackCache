namespace StackCache.Test
{
    using System.Collections.Generic;
    using System.Linq;
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
        private const string _test = "4242424244244242224442424242";
        private const string _test2 = "24424242442242424224442";

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
            List<CustomerDomain> result = (await this.Store.GetAll()).ToList();
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.False(result.Any(i => i?.Id == null));
        }

        [Fact]
        public async Task StoreSave_Create()
        {
            
            var created = new List<Crud<CustomerDomain>>
            {
                new Crud<CustomerDomain>(new CustomerDomain {Id = 42, Name = _test}, CrudAction.Insert)
            };
            await this.Store.Save(created);
            CustomerDomain found = await this.Store.Get(42);
            Assert.NotNull(found);
            Assert.Equal(found.Name, _test);
        }

        [Fact]
        public async Task StoreSave_Update()
        {
            var created = new List<Crud<CustomerDomain>>
            {
                new Crud<CustomerDomain>(new CustomerDomain {Id = 42, Name = _test2}, CrudAction.Update)
            };
            await this.Store.Save(created);
            CustomerDomain found = await this.Store.Get(42);
            Assert.NotNull(found);
            Assert.Equal(found.Name, _test2);
        }

        [Fact]
        public async Task StoreSave_Delete()
        {
            IEnumerable<CustomerDomain> result = await this.Store.GetAll();
            CustomerDomain one = result.FirstOrDefault(i => i.Id.HasValue);
            Assert.NotNull(one);
            var created = new List<Crud<CustomerDomain>> { new Crud<CustomerDomain>(one, CrudAction.Delete) };

            await this.Store.Save(created);

            Assert.NotNull(one.Id);
            CustomerDomain found = await this.Store.Get(one.Id.Value);
            Assert.Null(found);
        }
    }
}