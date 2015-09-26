namespace StackCache.Core.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class ConfigHelper
    {
        public static T LoadJson<T>(this FileInfo configFile) where T : Setting, new()
        {
            var config = new T();
            using (StreamReader sr = File.OpenText(configFile.FullName))
            {
                var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                return (T)serializer.Deserialize(sr, typeof(T));
            }
        }

        public static void GenerateSampleJson<T>(this FileInfo configFile, T configData) where T : Setting
        {
            using (StreamWriter sr = File.CreateText(configFile.FullName))
            {
                var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                serializer.Serialize(sr, configData, typeof(T));
            }
        }

        //public static T LoadConfig<T>(this FileInfo configFile) where T : Setting, new()
        //{
        //    var configMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile.FullName };
        //    return (T)ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None).GetSection("");
        //}

        //public static void GenerateSampleConfig<T>(this FileInfo configFile, T configData) where T : Setting
        //{
        //    var configMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile.FullName };
        //    configData.CurrentConfiguration.SaveAs(configFile.FullName, ConfigurationSaveMode.Full);
        //}

        public static void RegisterWatcher(this FileInfo configFile, Action callbackAction)
        {
            if (configFile == null) throw new ArgumentNullException(nameof(configFile));
            if (callbackAction == null) throw new ArgumentNullException(nameof(callbackAction));
            if (configFile.Directory == null) return;
            var watcher = new FileSystemWatcher(configFile.Directory.FullName, configFile.Name) { NotifyFilter = NotifyFilters.LastWrite };
            watcher.Changed += (s, args) =>
            {
                callbackAction();
            };
            watcher.EnableRaisingEvents = true;
        }


    }

    public class Settings
    {
        private static readonly Lazy<Settings> _lazy = new Lazy<Settings>(() => new Settings());
        private bool _isInitialized;
        private string _configFolder;

        private Settings()
        {
            this.ConfigFolder = AppDomain.CurrentDomain.BaseDirectory;
        }

        public static Settings Manager => _lazy.Value;

        /// <summary>
        /// Path where config files are stored
        /// </summary>
        public string ConfigFolder
        {
            get { return this._configFolder; }
            set
            {
                if (this._isInitialized)
                    throw new ConfigurationErrorsException(nameof(this.ConfigFolder) +
                                                           " must be configured before everything else");
                this._configFolder = value;
            }
        }

        private readonly ConcurrentDictionary<Type, Setting> _settings = new ConcurrentDictionary<Type, Setting>();
        private readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _settingProperties = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        public T GetSetting<T>()
            where T : Setting, new()
        {
            this._isInitialized = true;
            return (T)this._settings.GetOrAdd(typeof(T), (t) =>
            {
                string filenameJson = typeof(T).Name + ".json";
                //string filenameConfig = typeof(T).Name + ".config";

                var fileInfoJson = new FileInfo(Path.Combine(this.ConfigFolder, filenameJson));
                //var fileInfoConfig = new FileInfo(Path.Combine(this.ConfigFolder, filenameConfig));

                T config;
                if (fileInfoJson.Exists)
                {
                    config = fileInfoJson.LoadJson<T>();
                    fileInfoJson.RegisterWatcher(() =>
                    {
                        var newConfig = fileInfoJson.LoadJson<T>();
                        this.UpdateSetting(config, newConfig);
                        config.OnSettingFileChanged();
                    });
                }
                //else if (fileInfoConfig.Exists)
                //{
                //    config = fileInfoConfig.LoadConfig<T>();
                //    fileInfoConfig.RegisterWatcher(() =>
                //    {
                //        var newConfig = fileInfoConfig.LoadConfig<T>();
                //        this.UpdateSetting(config, newConfig);
                //        config.OnSettingFileChanged();
                //    });
                //}
                else
                    throw new Exception("No " + typeof(T).Name + ".config" + " or " + typeof(T).Name + ".json file found");

                return config;
            });
        }

        internal void GenerateSampleConfigFiles()
        {
            Setting[] configsSettings =
            {
                new RedisSetting()
            };

            foreach (Setting t in configsSettings)
            {
                var fileInfoJson = new FileInfo(Path.Combine(this.ConfigFolder, t.GetType().Name + ".json.example"));
                fileInfoJson.GenerateSampleJson(t);
                //var fileInfoConfig = new FileInfo(Path.Combine(this.ConfigFolder, t.GetType().Name + ".config.example"));
                //fileInfoConfig.GenerateSampleConfig(t);
            }
        }

        /// <summary>
        /// Inspired from Nick Craver OpServer Code
        /// </summary> 
        private bool UpdateSetting<T>(T referenceInstance, T newValues)
            where T : Setting
        {
            if (referenceInstance == null || newValues == null) return false;
            IEnumerable<PropertyInfo> props = this._settingProperties
                .GetOrAdd(typeof(T), tt => tt.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));
            bool changed = false;
            try
            {
                foreach (PropertyInfo prop in props)
                {
                    if (!prop.CanWrite) continue;
                    object current = prop.GetValue(this);
                    object newSetting = prop.GetValue(newValues);
                    try
                    {
                        if (current == newSetting) continue;
                        prop.SetValue(this, newSetting);
                        changed = true;
                    }
                    catch (Exception e)
                    {
                        //Current.LogException("Error setting propery: " + prop.Name, e);
                    }
                }
            }
            catch (Exception e)
            {
                //Current.LogException("Error updating settings for " + typeof(T).Name, e);
            }

            if (changed)
                referenceInstance.OnSettingFileChanged();
            return changed;
        }
    }


    public abstract class Setting
    {
        public virtual void OnSettingFileChanged()
        {
        }
    }

    public class RedisSetting : Setting
    {
        public String ServerName
        {
            get; set;
        }

        private IEnumerable<RedisServer> redisInstances;


    }
}