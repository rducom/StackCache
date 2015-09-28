namespace StackCache.Test
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Configuration;
    using Xunit;

    public class Config
    {
        private readonly string _root;

        public Config()
        {
            var basetests = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            if (basetests.Parent?.Parent != null)
                this._root = basetests.Parent.Parent.FullName;

            this._configsSettings = new Setting[]
        {
                new RedisSetting
                {
                    ServerName = "localhost",
                    RedisInstances = new List<RedisServer>
                    {
                        new RedisServer {Hostname = "127.0.0.1", Port = 6379}
                    }
                }
            };
        }

        private readonly Setting[] _configsSettings;
        [Fact]
        public void GenerateSampleConfigFiles()
        {


            Settings.Manager.SaveConfigFiles(this._configsSettings);
        }

        [Fact]
        public void LoadDefaultConfigs()
        {
            Settings.Manager.ConfigFolder = this._root;
            RedisSetting found = Settings.Manager.GetSetting<RedisSetting>();

            Assert.NotNull(found);
            Assert.False(string.IsNullOrEmpty(found.ServerName));
            Assert.NotNull(found.RedisInstances);

            RedisServer host = found.RedisInstances.FirstOrDefault();
            Assert.NotNull(host);
            Assert.False(string.IsNullOrEmpty(host.Hostname));
        }

        [Fact]
        public void TestAlreadyInitializedSettings()
        {
            Settings.Manager.ConfigFolder = this._root;
            RedisSetting found = Settings.Manager.GetSetting<RedisSetting>();
            Settings.Manager.ConfigFolder = this._root;

            Assert.Throws<ConfigurationErrorsException>(() => { Settings.Manager.ConfigFolder = this._root + @"\1\"; });
        }

    }

    public class ConfigOverload
    {

        [Fact]
        public async Task TestReloadSettings()
        {
            // test in \Debug\bin\
            Settings.Manager.ConfigFolder = AppDomain.CurrentDomain.BaseDirectory;
            var rs = new RedisSetting
            {
                ServerName = "localhost",
                RedisInstances = new List<RedisServer>
                {
                    new RedisServer {Hostname = "127.0.0.1", Port = 6379}
                }
            };
            Settings.Manager.SaveConfigFiles(new Setting[] { rs });

            await Task.Delay(100);

            RedisSetting found = Settings.Manager.GetSetting<RedisSetting>();

            Setting[] testSettings =
            {
                new RedisSetting
                {
                    ServerName = "localhost_test_update",
                    RedisInstances = new List<RedisServer>
                    {
                        new RedisServer {Hostname = "127.0.0.1", Port = 6379}
                    }
                }
            };

            bool settingChanged = false;
            found.SettingFileChanged += (s, e) =>
            {
                settingChanged = true;
            };

            Settings.Manager.SaveConfigFiles(testSettings);

            await Task.Delay(100);

            Assert.True(settingChanged);
            Assert.True(found.ServerName == "localhost_test_update");
        }
    }
}