namespace StackCache.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.CacheKeys;
    using Core.Serializers;
    using Xunit;

    public class KeyTests
    {
        [Fact]
        public void Affectation()
        {
            Key k1 = "ppppp";
            KeyPrefix p1 = new KeyPrefix(Key.Null, "1651651645621");
            CacheKey cc = new CacheKey(p1, k1);
            CacheKey affected = cc;
            Assert.True(cc == affected);
            var kv = new KeyValuePair<CacheKey, object>(affected, null);
            Assert.Equal(cc, kv.Key);
        }

        [Fact]
        public void KeyHash()
        {
            CacheKey k1 = new CacheKey(string.Empty, "646846516584", "57");
            Dictionary<CacheKey, object> dic = new Dictionary<CacheKey, object>();
            dic.Add(k1, new object());
            var found = dic[k1];
            Assert.NotNull(found);
        }

        [Fact]
        public void KeyTranstypeString()
        {
            Key k1 = "test";
            Assert.True(k1 == "test");
        }

        [Fact]
        public void KeyPrefixTranstypeString()
        {
            KeyPrefix p1 = new KeyPrefix("test", Key.Null);
            Assert.True(p1.Tenant == "test");
        }

        [Fact]
        public void KeyPrefixTenantTranstypeString()
        {
            KeyPrefix p1 = new KeyPrefix("tenant", "test");
            Assert.True(p1.Tenant == "tenant" && p1.Region == "test");
        }


        [Fact]
        public void KeyComposition()
        {
            Key k1 = "test";
            KeyPrefix p1 = new KeyPrefix("tenant", "region");
            CacheKey cc = new CacheKey(p1, k1);
            Assert.True(cc.Prefix.Tenant == "tenant" && cc.Prefix.Region == "region" && cc.Suffix == "test");
            CacheKey newcc = cc;
            Assert.True(newcc == cc);
        }

        [Fact]
        public void KeySerializes()
        {
            CacheKey cc = new CacheKey("prefix", "region", "tenant");
            var iserial = typeof(ISerializer);
            foreach (Type serializerType in AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("StackCache"))
                .SelectMany(s => s.GetTypes())
                .Where(p => iserial.IsAssignableFrom(p) && !p.IsAbstract))
            {
                ISerializer serializer = (ISerializer)Activator.CreateInstance(serializerType);
                var data = serializer.Serialize(cc);
                CacheKey newcc = serializer.Deserialize<CacheKey>(data);
                Assert.True(cc == newcc);
            }
        }
    }
}