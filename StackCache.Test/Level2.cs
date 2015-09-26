namespace StackCache.Test
{
    using Fixture;
    using Xunit;
    using Xunit.Abstractions;

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
}