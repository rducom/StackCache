namespace StackCache.Test
{
    using System;
    using Core;
    using Core.Configuration;
    using Xunit;

    public class Setup
    {
        private const string _key = "test";
        private const string _keydata = "testdata";

        [Fact]
        public void Create_ZeroConfig()
        {
            CacheConfiguration config = new CacheConfiguration();
            ICache cache = config.CreateCache();
            Assert.NotNull(cache);
            CanPutThenGet(cache);
        }


        private static void CanPutThenGet(ICache cache)
        {
            cache.Put(_key, _keydata);
            var found = cache.Get<string>(_key);
            Assert.Equal(found, _keydata);
        }

        [Fact]
        public void ConfigLevel1_Concurrent()
        {
            CacheConfiguration config = new CacheConfiguration();
            config.WithFirstLevel(FirstLevelCacheType.ConcurrentDictionary);
            var cache = config.CreateCache();
            Assert.NotNull(cache);
            CanPutThenGet(cache);
        }

        //[Fact]
        //public void ConfigLevel1_MemoryCache()
        //{
        //    CacheConfiguration config = new CacheConfiguration();
        //    config.WithFirstLevel(FirstLevelCacheType.MemoryCache);
        //    var cache = config.CreateCache();
        //    Assert.NotNull(cache);
        //}

        //[Fact]
        //public void ConfigLevel1_LockedDic()
        //{
        //    CacheConfiguration config = new CacheConfiguration();
        //    config.WithFirstLevel(FirstLevelCacheType.LockedDictionary);
        //    var cache = config.CreateCache();
        //    Assert.NotNull(cache);
        //}

        [Fact]
        public void ConfigLevel2_EmptyConfig1()
        {
            CacheConfiguration config = new CacheConfiguration();
            ICache cache = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                config.WithSecondLevel(0);
                cache = config.CreateCache();
            });
            Assert.Null(cache);
        }

        [Fact]
        public void ConfigLevel2_EmptyConfig2()
        {
            CacheConfiguration config = new CacheConfiguration();
            ICache cache = null;
            Assert.Throws<ArgumentException>(() =>
            {
                config.WithSecondLevel(0, new RedisServer());
                cache = config.CreateCache();
            });
            Assert.Null(cache);
        }

        [Fact]
        public void ConfigLevel2_EmptyConfig3()
        {
            CacheConfiguration config = new CacheConfiguration();
            ICache cache = null;
            Assert.Throws<ArgumentException>(() =>
            {
                config.WithSecondLevel(0, new RedisServer());
                cache = config.CreateCache();
            });
            Assert.Null(cache);
        }
    }
}