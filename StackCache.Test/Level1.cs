using System.Collections.Generic;
using System.Threading.Tasks; 
using Caching.Test.Fixture;
using ProtoBuf;
using Xunit;
using Xunit.Abstractions;

namespace Caching.Test
{
    using Data;
    using StackCache.Core;
    using StackCache.Core.Configuration;
    using Store;

    [ProtoContract]
    public class Serialized
    {
        [ProtoMember(1)]
        public string Property { get; set; }
    }



    /// <summary>
    /// Check cache without second level adapter
    /// </summary>
    [Collection(Consts.Level1)]
    public class Level1 : CacheTest
    {
        public Level1(Level1Fixture fix, ITestOutputHelper output)
            : base(fix, output)
        {
        }
    }


    /// <summary>
    /// Check cache wit second level adapter
    /// </summary>
    [Collection(Consts.Level2)]
    public class Level2 : CacheTest
    {
        public Level2(Level2Fixture fix, ITestOutputHelper output)
            : base(fix, output)
        {
        }
    }



    public class ConcurentCaches
    {
        private const string _keySerialized = "42Concurent";
        private static readonly Serialized _dataSerialized = new Serialized() { Property = "forty two concurent property" };

        public ConcurentCaches()
        {
            CacheConfiguration config = new CacheConfiguration();
            config.WithFirstLevel(FirstLevelCacheType.ConcurrentDictionary);
            config.WithSecondLevel(0, new RedisInstance() { Hostname = "127.0.0.1", Port = 6379 });

            this._cache1 = config.CreateCache();
            this._cache2 = config.CreateCache();
        }

        private ICache _cache1;
        private ICache _cache2;

        [Fact]
        public void GetPut()
        {
            this._cache1.Put(_keySerialized, _dataSerialized);
            var found = this._cache2.Get<Serialized>(_keySerialized);
            Assert.Equal(found.Property, _dataSerialized.Property);
        }


        [Fact]
        public void GetPutUpdate()
        {
            var key = _keySerialized + "UP";
            this._cache1.Put(key, _dataSerialized);
            var found = this._cache2.Get<Serialized>(key);
            Assert.Equal(found.Property, _dataSerialized.Property);

            var initial = this._cache1.Get<Serialized>(key);
            initial.Property = "666";
            this._cache1.Put(key, initial);

            var updated = this._cache2.Get<Serialized>(key);
            Assert.Equal(updated.Property, "666");
        }
    }

    /// <summary>
    /// Check store without second level adapter
    /// </summary>
    [Collection(Consts.Level1)]
    public class StoreL1 : CacheTest
    {
        public StoreL1(Level1Fixture fix, ITestOutputHelper output)
            : base(fix, output)
        {
        }

        [Fact]
        public async Task Test()
        {
            CacheConfiguration config = new CacheConfiguration()
                .WithFirstLevel(FirstLevelCacheType.ConcurrentDictionary);

            ICache cache = config.CreateCache();

            var store = new CustomerStore(cache);

            var alls = await store.GetAll();

            var cust = new CustomerDomain { Id = 42, Name = "Quarante deux" };

            CustomerDomain get = await store.Get(42);

            IEnumerable<CustomerDomain> all = await store.GetAll();
        }
    }



    /// <summary>
    /// Check store without second level adapter
    /// </summary>
    [Collection(Consts.Level2)]
    public class StoreL2 : CacheTest
    {
        public StoreL2(Level2Fixture fix, ITestOutputHelper output)
            : base(fix, output)
        {
        }

        [Fact]
        public async Task Test()
        {
            CacheConfiguration config = new CacheConfiguration()
                .WithFirstLevel(FirstLevelCacheType.ConcurrentDictionary)
                .WithSecondLevel(0, new RedisInstance() { Hostname = "127.0.0.1", Port = 6379 });

            ICache cache = config.CreateCache();

            var store = new CustomerStore(cache);

            var alls = await store.GetAll();

            var cust = new CustomerDomain { Id = 42, Name = "Quarante deux" };

            CustomerDomain get = await store.Get(42);

            IEnumerable<CustomerDomain> all = await store.GetAll();
        }
    }

}
