namespace StackCache.Test
{
    using Fixture;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Check store without second level adapter
    /// </summary>
    [Collection(Consts.Level1)]
    public class StoreL1 : StoreTest
    {
        public StoreL1(Level1Fixture fix, ITestOutputHelper output)
            : base(fix, output)
        {
        }
    }
}