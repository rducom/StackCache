namespace StackCache.Test.Fixture
{
    using Core.Configuration;
    using Core.Election;

    public class Level1Fixture : CacheFixture
    {
        public Level1Fixture()
        {
            CacheConfiguration config = new CacheConfiguration()
                .WithFirstLevel(FirstLevelCacheType.ConcurrentDictionary);
            this.Cache = config.CreateCache();
            this.Elector = new ConfiguredElection(true);
        }
    }
}
