namespace StackCache.Core.Configuration
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal static class ConfigHelper
    {


        internal static T LoadAsJson<T>(this FileInfo configFile) where T : Setting, new()
        { 
            try
            {
                using (StreamReader sr = File.OpenText(configFile.FullName))
                {
                    var serializer = new JsonSerializer
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };
                    return (T) serializer.Deserialize(sr, typeof (T));
                }
            }
            catch (Exception)
            {
                // TODO : log
            }
            return default(T);
        }

        internal static void SaveAsJson<T>(this FileInfo configFile, T configData) where T : Setting
        {
            try
            {
                using (StreamWriter sr = File.CreateText(configFile.FullName))
            {
                var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                serializer.Serialize(sr, configData, typeof(T));
            }
            }
            catch (Exception)
            {
                // TODO : log
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

        internal static void RegisterWatcher(this FileInfo configFile, Action callbackAction)
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
}