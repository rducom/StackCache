

namespace StackCache.Test.Fixture
{
    using Core;
    using Core.Configuration;
    using Core.Election;

    public class Level2Fixture : CacheFixture
    {

        public Level2Fixture()
        {
            CacheConfiguration config = new CacheConfiguration()
                 .WithFirstLevel(FirstLevelCacheType.ConcurrentDictionary)
                 .WithSecondLevel(0, new RedisServer() { Hostname = "127.0.0.1", Port = 6379 });

            Cache cc = config.CreateCache();
            this.Cache = cc;
            this.Elector = new DistributedMutexElection(cc.DistributedCache);
        }

    }
}