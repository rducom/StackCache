namespace StackCache.Test
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Core.Configuration;
    using Xunit;

    public class ConfigBasics
    {
        private readonly Setting[] _configsSettings;
        private readonly string _root;
        private readonly SettingsManager _settingsManager;

        public ConfigBasics()
        {
            this._settingsManager = SettingsManager.NewSettingsManager();
            var basetests = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            if (basetests.Parent?.Parent != null)
                this._root = basetests.Parent.Parent.FullName;
            this._settingsManager.ConfigFolder = this._root;
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

        [Fact]
        public void GenerateSampleConfigFiles()
        {
            this._settingsManager.SaveConfigFiles(this._configsSettings);
        }

        [Fact]
        public void LoadDefaultConfigs()
        {
            RedisSetting found = this._settingsManager.GetSetting<RedisSetting>();

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
            this._settingsManager.GetSetting<RedisSetting>(); // ensure initialized
            Assert.Throws<ConfigurationErrorsException>(
                () =>
                {
                    this._settingsManager.ConfigFolder = this._root + @"\1\"; 
                    
                });
        }
    }
}