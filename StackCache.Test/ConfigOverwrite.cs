namespace StackCache.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Configuration;
    using Xunit;

    public class ConfigOverwrite
    {
        private readonly SettingsManager _settingsManager;

        public ConfigOverwrite()
        {
            this._settingsManager = SettingsManager.NewSettingsManager();
        }

        [Fact]
        public async Task TestReloadSettings()
        {
            // test in \Debug\bin\
            this._settingsManager.ConfigFolder = AppDomain.CurrentDomain.BaseDirectory;
            var rs = new RedisSetting
            {
                ServerName = "localhost",
                RedisInstances = new List<RedisServer>
                {
                    new RedisServer {Hostname = "127.0.0.1", Port = 6379}
                }
            };
            this._settingsManager.SaveConfigFiles(new Setting[] {rs});

            await Task.Delay(100);

            var found = this._settingsManager.GetSetting<RedisSetting>();

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

            var settingChanged = false;
            found.SettingFileChanged += (s, e) => { settingChanged = true; };

            this._settingsManager.SaveConfigFiles(testSettings);

            await Task.Delay(100);

            Assert.True(settingChanged);
            Assert.Equal("localhost_test_update",found.ServerName );
        }
    }
}