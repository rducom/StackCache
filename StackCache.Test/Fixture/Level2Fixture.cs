

namespace Caching.Test.Fixture
{
    using System.Net;
    using StackCache.Core.Configuration;

    public class Level2Fixture : CacheFixture
    {

        public Level2Fixture()
        {
            CacheConfiguration config = new CacheConfiguration()
                 .WithFirstLevel(FirstLevelCacheType.ConcurrentDictionary)
                 .WithSecondLevel(0, new RedisInstance() { Hostname = "127.0.0.1", Port = 6379 });
            this.Cache = config.CreateCache(); ;
        }

    }
}