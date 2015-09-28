namespace StackCache.Core.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Reflection;

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
                if (this._isInitialized && this._configFolder != value)
                    throw new ConfigurationErrorsException(nameof(this.ConfigFolder) +
                                                           " must be configured before everything else");
                this._configFolder = value;
            }
        }

        private readonly ConcurrentDictionary<Type, Setting> _settings = new ConcurrentDictionary<Type, Setting>();
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _settingProperties = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

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
                    config = fileInfoJson.LoadAsJson<T>();
                    fileInfoJson.RegisterWatcher(() =>
                    {
                        var newConfig = fileInfoJson.LoadAsJson<T>();
                        RefreshSetting(config, newConfig);
                        config.OnSettingFileChanged();
                    });
                }
                else
                    throw new Exception("No " + typeof(T).Name + ".config" + " or " + typeof(T).Name + ".json file found");

                return config;
            });
        }

        public void SaveConfigFiles(Setting[] configsSettings)
        {

            foreach (Setting t in configsSettings)
            {
                var fileInfoJson = new FileInfo(Path.Combine(this.ConfigFolder, t.GetType().Name + ".json"));
                fileInfoJson.SaveAsJson(t);
            }
        }

        /// <summary>
        /// Inspired from Nick Craver OpServer Code
        /// </summary> 
        private static bool RefreshSetting<T>(T referenceInstance, T newValues)
            where T : Setting
        {
            if (referenceInstance == null || newValues == null) return false;
            IEnumerable<PropertyInfo> props = _settingProperties
                .GetOrAdd(typeof(T), tt => tt.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));
            bool changed = false;
            try
            {
                foreach (PropertyInfo prop in props)
                {
                    if (!prop.CanWrite || !prop.CanRead) continue;
                    object current = prop.GetValue(referenceInstance);
                    object newSetting = prop.GetValue(newValues);
                    if (current == newSetting) continue;
                    prop.SetValue(referenceInstance, newSetting);
                    changed = true;
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
}