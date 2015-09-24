using Xunit;

namespace Caching.Test.Fixture
{
    [CollectionDefinition(Consts.Level1)]
    public class Level1Test : ICollectionFixture<Level1Fixture> { }
}