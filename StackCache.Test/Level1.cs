namespace StackCache.Test
{
    using Fixture;
    using Xunit;
    using Xunit.Abstractions;

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
}
