using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Caching.Test
{
    using StackCache.Core.Locking;

    public class AsyncTest
    {
        private static int iterations = 4096 * 2;

        [Fact]
        public async Task v1()
        {
            int count = 0;

            Func<Task<int>> funk = async () =>
            {
                Interlocked.Increment(ref count);
                await Task.Delay(100);
                return 42;
            };

            var once = funk.OnlyOnce();

            await Task.WhenAll(Enumerable.Range(0, iterations).Select(async i =>
            {
                int found = await once.ExclusiveAsync_v1();
                Assert.Equal(found, 42);
            }));

            int foundBis = await once.ExclusiveAsync_v1();
            Assert.Equal(foundBis, 42);
            Assert.Equal(count, 1);
        }


        [Fact]
        public async Task v2()
        {
            int count = 0;

            Func<Task<int>> funk = async () =>
            {
                Interlocked.Increment(ref count);
                await Task.Delay(100);
                return 42;
            };

            var once = funk.OnlyOnce();

            await Task.WhenAll(Enumerable.Range(0, iterations).Select(async i =>
            {
                int found = await once.ExclusiveAsync_v2();
                Assert.Equal(found, 42);
            }));

            int foundBis = await once.ExclusiveAsync_v2();
            Assert.Equal(foundBis, 42);
            Assert.Equal(count, 1);
        }


        [Fact]
        public async Task v3()
        {
            int count = 0;

            Func<Task<int>> funk = async () =>
            {
                Interlocked.Increment(ref count);
                await Task.Delay(100);
                return 42;
            };

            var once = funk.OnlyOnce();

            await Task.WhenAll(Enumerable.Range(0, iterations).Select(async i =>
            {
                int found = await once.ExclusiveAsync_v3();
                Assert.Equal(found, 42);
            }));

            int foundBis = await once.ExclusiveAsync_v3();
            Assert.Equal(foundBis, 42);
            Assert.Equal(count, 1);
        }


        [Fact]
        public async Task v4()
        {
            int count = 0;

            Func<Task<int>> funk = async () =>
            {
                Interlocked.Increment(ref count);
                await Task.Delay(100);
                return 42;
            };

            var once = funk.OnlyOnce();

            await Task.WhenAll(Enumerable.Range(0, iterations).Select(async i =>
            {
                int found = await once.ExclusiveAsync_v4();
                Assert.Equal(found, 42);
            }));

            int foundBis = await once.ExclusiveAsync_v4();
            Assert.Equal(foundBis, 42);
            Assert.Equal(count, 1);
        }


        [Fact]
        public async Task v5()
        {
            int count = 0;

            Func<Task<int>> funk = async () =>
            {
                Interlocked.Increment(ref count);
                await Task.Delay(100);
                return 42;
            };

            var once = new AsyncLazy<int>(funk);

            await Task.WhenAll(Enumerable.Range(0, iterations).Select(async i =>
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
