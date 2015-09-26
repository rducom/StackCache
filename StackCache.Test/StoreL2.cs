namespace StackCache.Test
{
    using Fixture;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Check store without second level adapter
    /// </summary>
    [Collection(Consts.Level2)]
    public class StoreL2 : StoreTest
    {
        public StoreL2(Level2Fixture fix, ITestOutputHelper output)
            : base(fix, output)
        {
        }
    }
}