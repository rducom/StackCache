namespace StackCache.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fixture;
    using Core;
    using Core.CacheKeys;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class CacheTest
    {
        private readonly CacheFixture _fix;
        private readonly ITestOutputHelper _output;

        protected CacheTest(CacheFixture fix, ITestOutputHelper output)
        {
            this._fix = fix;
            this._output = output;
        }

        private ICache _cache => this._fix.Cache;

        private const string _keyString = "42String";
        private const string _dataString = "forty two";

        private const string _keyInteger = "42Int";
        private const int _dataInteger = 42;

        private const string _keySerialized = "42Serial";
        private static readonly Serialized _dataSerialized = new Serialized { Property = "forty two property" };

        [Fact]
        public void PutGetString()
        {
            this._cache.Put(_keyString, _dataString);
            var found = this._cache.Get<string>(_keyString);
            Assert.Equal(found, _dataString);
        }


        [Fact]
        public void CacheDefault()
        {
            var d1 = Cache.Default;
            var d2 = Cache.Default;
            Assert.Equal(d1, d2);
        }

        [Fact]
        public void PutGetInt()
        {
            this._cache.Put(_keyInteger, _dataInteger);
            var found = this._cache.Get<int>(_keyInteger);
            Assert.Equal(found, _dataInteger);
        }


        [Fact]
        public void PutGetOrCreate()
        {
            var temp1 = this._cache.GetOrCreate("42", (k) => 42);
            var temp2 = this._cache.GetOrCreate("42", (k) => 42);
            Assert.Equal(temp1, temp2);
        }

        [Fact]
        public void PutGetSerialized()
        {
            this._cache.Put(_keySerialized, _dataSerialized);
            var found = this._cache.Get<Serialized>(_keySerialized);
            Assert.Equal(found.Property, _dataSerialized.Property);
        }

        [Fact]
        public void GetRegionArgument()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                this._cache.GetRegion<String>(new KeyPrefix("123", "456"), null);
            });
        }

        [Fact]
        public void GetEmptyRegion()
        {
            Func<Serialized, CacheKey> funk = s => new CacheKey(new KeyPrefix("8754", "9765"), s.Id.ToString());
            IEnumerable<Serialized> result = this._cache.GetRegion(new KeyPrefix("123", "456"), funk);
            Assert.Empty(result);
        }


        [Fact]
        public async Task GetRegionAsyncArgument()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await this._cache.GetRegionAsync<String>(new KeyPrefix("123", "456"), null);
            });
        }

        [Fact]
        public async Task GetEmptyRegionAsync()
        {
            Func<Serialized, CacheKey> funk = s => new CacheKey(new KeyPrefix("8754", "9765"), s.Id.ToString());
            IEnumerable<Serialized> result = await this._cache.GetRegionAsync(new KeyPrefix("123", "456"), funk);
            Assert.Empty(result);
        }

        [Fact]
        public async Task PutGetStringRegion()
        {
            KeyPrefix prefix = new KeyPrefix(Key.Null, _keyString);
            Dictionary<CacheKey, string> dictionary = Enumerable.Range(0, 100)
                .ToDictionary(i => new CacheKey(prefix, i.ToString()), i => _dataString + i.ToString());
            this._cache.PutRegion(dictionary.ToArray());

            var fetched = await this._cache.GetRegionKeyValuesAsync<string>(prefix);
            Assert.Equal(dictionary.Count(), fetched.Count());
            foreach (var kv in fetched)
            {
                Assert.Equal(dictionary[kv.Key], kv.Value);
            }
        }

        [Fact]
        public async Task PutGetSerializedRegion()
        {
            KeyPrefix prefix = new KeyPrefix(Key.Null, _keyString);

            Dictionary<CacheKey, Serialized> dictionary = Enumerable.Range(0, 100)
                .ToDictionary(i => new CacheKey(prefix, i.ToString()), i => new Serialized { Property = _dataString + i.ToString() });

            this._cache.PutRegion(dictionary.ToArray());
            var fetched = await this._cache.GetRegionKeyValuesAsync<Serialized>(prefix);
            Assert.Equal(dictionary.Count(), fetched.Count());
            foreach (var kv in fetched)
            {
                Assert.Equal(dictionary[kv.Key].Property, kv.Value.Property);
            }
        }

        [Fact]
        public void CanRemove()
        {
            this._cache.Put(_keyString, _dataString);
            var found = this._cache.Get<string>(_keyString);
            Assert.Equal(found, _dataString);
            this._cache.Remove(_keyString);
            Assert.Null(this._cache.Get<string>(_keyString));
        }

        [Fact]
        public async Task CanRemoveRegion()
        {
            KeyPrefix prefix = new KeyPrefix(Key.Null, _keyString);
            Dictionary<CacheKey, string> dictionary = Enumerable.Range(0, 100)
                .ToDictionary(i => new CacheKey(prefix, i.ToString()), i => _dataString + i.ToString());
            this._cache.PutRegion(dictionary.ToArray());
            var fetched = await this._cache.GetRegionKeyValuesAsync<string>(prefix);
            Assert.Equal(dictionary.Count(), fetched.Count());
            this._cache.RemoveRegion(prefix);
            IEnumerable<KeyValuePair<CacheKey, Serialized>> removed = await this._cache.GetRegionKeyValuesAsync<Serialized>(prefix);
            Assert.Empty(removed);
        }

        [Fact]
        public void EnsureGetExceptionWhenBadType()
        {
            Assert.ThrowsAny<Exception>(() =>
            {
                this._cache.Put(_keySerialized, _dataSerialized);
                this._cache.Get<string>(_keySerialized);
            });
        }

    }
}