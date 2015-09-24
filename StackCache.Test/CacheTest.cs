using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.Test.Fixture;
using Xunit;
using Xunit.Abstractions;

namespace Caching.Test
{
    using StackCache.Core;
    using StackCache.Core.CacheKeys;

    public abstract class CacheTest
    {
        private readonly CacheFixture _fix;
        private readonly ITestOutputHelper _output;

        protected CacheTest(CacheFixture fix, ITestOutputHelper output)
        {
            this._fix = fix;
            this._output = output;
        }

        protected ICache Cache => this._fix.Cache;

        private const string _keyString = "42String";
        private const string _dataString = "forty two";

        private const string _keyInteger = "42Int";
        private const int _dataInteger = 42;

        private const string _keySerialized = "42Serial";
        private static readonly Serialized _dataSerialized = new Serialized() { Property = "forty two property" };

        [Fact]
        public void PutGetString()
        {
            this.Cache.Put(_keyString, _dataString);
            var found = this.Cache.Get<string>(_keyString);
            Assert.Equal(found, _dataString);
        }

        [Fact]
        public void PutGetInt()
        {
            this.Cache.Put(_keyInteger, _dataInteger);
            var found = this.Cache.Get<int>(_keyInteger);
            Assert.Equal(found, _dataInteger);
        }

        [Fact]
        public void PutGetSerialized()
        {
            this.Cache.Put(_keySerialized, _dataSerialized);
            var found = this.Cache.Get<Serialized>(_keySerialized);
            Assert.Equal(found.Property, _dataSerialized.Property);
        }

        [Fact]
        public async Task PutGetStringRegion()
        {
            KeyPrefix prefix = new KeyPrefix(KeyPrefix.Null, _keyString);
            Dictionary<CacheKey, string> dictionary = Enumerable.Range(0, 100)
                .ToDictionary(i => new CacheKey(prefix, i.ToString()), i => _dataString + i.ToString());
            this.Cache.PutRegion(dictionary.ToArray());

            var fetched = await this.Cache.GetRegionKeyValuesAsync<string>(prefix);
            Assert.Equal(dictionary.Count(), fetched.Count());
            foreach (var kv in fetched)
            {
                Assert.Equal(dictionary[kv.Key], kv.Value);
            }
        }

        [Fact]
        public async Task PutGetSerializedRegion()
        {
            KeyPrefix prefix = new KeyPrefix(KeyPrefix.Null, _keyString);

            Dictionary<CacheKey, Serialized> dictionary = Enumerable.Range(0, 100)
                .ToDictionary(i => new CacheKey(prefix, i.ToString()), i => new Serialized() { Property = _dataString + i.ToString() });

            this.Cache.PutRegion(dictionary.ToArray());
            var fetched = await this.Cache.GetRegionKeyValuesAsync<Serialized>(prefix);
            Assert.Equal(dictionary.Count(), fetched.Count());
            foreach (var kv in fetched)
            {
                Assert.Equal(dictionary[kv.Key].Property, kv.Value.Property);
            }
        }

        //[Fact]
        //public void ConcurentPut_Optimistic()
        //{
        //}

        //[Fact]
        //public void ConcurentPut_Pessimistic()
        //{
        //}

        //[Fact]
        //public void ExpiresAtFixedDate()
        //{
        //}

        //[Fact]
        //public void ExpiresAfterTimespan()
        //{
        //}

        [Fact]
        public void CanRemove()
        {
            this.Cache.Put(_keyString, _dataString);
            var found = this.Cache.Get<string>(_keyString);
            Assert.Equal(found, _dataString);
            this.Cache.Remove(_keyString);
            Assert.Null(this.Cache.Get<string>(_keyString));
        }

        [Fact]
        public async Task CanRemoveRegion()
        {
            KeyPrefix prefix = new KeyPrefix(KeyPrefix.Null, _keyString);
            Dictionary<CacheKey, string> dictionary = Enumerable.Range(0, 100)
                .ToDictionary(i => new CacheKey(prefix, i.ToString()), i => _dataString + i.ToString());
            this.Cache.PutRegion(dictionary.ToArray());
            var fetched = await this.Cache.GetRegionKeyValuesAsync<string>(prefix);
            Assert.Equal(dictionary.Count(), fetched.Count());
            this.Cache.RemoveRegion(prefix);
            IEnumerable<KeyValuePair<CacheKey, Serialized>> removed = await this.Cache.GetRegionKeyValuesAsync<Serialized>(prefix);
            Assert.Empty(removed);
        }

        [Fact]
        public void EnsureGetExceptionWhenBadType()
        {
            Assert.ThrowsAny<Exception>(() =>
            {
                this.Cache.Put(_keySerialized, _dataSerialized);
                this.Cache.Get<string>(_keySerialized);
            });
        }

    }
}