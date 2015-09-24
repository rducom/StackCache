using Xunit;

namespace Caching.Test.Fixture
{
    [CollectionDefinition(Consts.Level2)]
    public class Level2Test : ICollectionFixture<Level2Fixture> { }
}