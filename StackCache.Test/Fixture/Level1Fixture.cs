namespace Caching.Test.Fixture
{
    using StackCache.Core.Configuration;

    public class Level1Fixture : CacheFixture
    {
        public Level1Fixture()
        {
            CacheConfiguration config = new CacheConfiguration()
                .WithFirstLevel(FirstLevelCacheType.ConcurrentDictionary);
            this.Cache = config.CreateCache();
        }
    }
}
