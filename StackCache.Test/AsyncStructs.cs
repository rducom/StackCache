namespace StackCache.Test
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Locking;
    using Xunit;

    public class AsyncStructs
    {
        private static int parallelAwaits = 4096 * 2;
         
        [Fact]
        public async Task AsyncExclusiveTest()
        {
            int count = 0;

            Func<Task<int>> funk = async () =>
            {
                Interlocked.Increment(ref count);
                await Task.Delay(100);
                return 42;
            };

            AsyncExclusive<int> once = funk.OnlyOnce();

            await Task.WhenAll(Enumerable.Range(0, parallelAwaits).Select(async i =>
            {
                int found = await once.Exclusive();
                Assert.Equal(found, 42);
            }));

            int foundBis = await once.Exclusive();
            Assert.Equal(foundBis, 42);
            Assert.Equal(count, 1);
        }


        [Fact]
        public async Task AsyncLazyTest()
        {
            int count = 0;

            Func<Task<int>> funk = async () =>
            {
                Interlocked.Increment(ref count);
                await Task.Delay(100);
                return 42;
            };

            var once = new AsyncLazy<int>(funk);

            await Task.WhenAll(Enumerable.Range(0, parallelAwaits).Select(async i =>
            {
                int found = await once;
                Assert.Equal(found, 42);
            }));

            int foundBis = await once;
            Assert.Equal(foundBis, 42);
            Assert.Equal(count, 1);
        }
    }
}
