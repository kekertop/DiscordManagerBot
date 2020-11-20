using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordChannelsBot.Services
{
    public class ConfigurationService
    {
        protected const string SETTINGS_FILE_NAME = "appSettings.json";
        protected Dictionary<String, object> properties;
        public event Action ConfigurationUpdated;
        public ConfigurationService()
        {
            properties = JsonConvert.DeserializeObject<Dictionary<String, Object>>(File.ReadAllText(SETTINGS_FILE_NAME));
            using FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = Directory.GetCurrentDirectory();
            watcher.Filter = SETTINGS_FILE_NAME;
            watcher.Changed += ConfigurationFileUpdate;
        }

        private void ConfigurationFileUpdate(object sender, FileSystemEventArgs e)
        {
            properties = JsonConvert.DeserializeObject<Dictionary<String, Object>>(File.ReadAllText(SETTINGS_FILE_NAME));
            ConfigurationUpdated.Invoke();
        }

        public TResult GetProperty<TResult>(String propertyName)
        {
            if (properties.ContainsKey(propertyName))
            {
                Object result = properties[propertyName];

                if (result is TResult)
                {
                    return (TResult)result;
                }
                else
                {
                    TResult finalResult;
                    try
                    {
                        finalResult = JsonConvert.DeserializeObject<TResult>(JsonConvert.SerializeObject(result));
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidCastException($"The property is not {typeof(TResult).Name} instance.");
                    }
                    return finalResult;

                }
            }
            throw new KeyNotFoundException($"No property named ${propertyName} found");

        }
        public async Task SetPropertyAsync(string propertyName, object value)
        {
            if (properties.ContainsKey(propertyName))
            {
                properties[propertyName] = value;
            }
            else
            {
                properties.Add(propertyName, value);
            }
            await SaveAsync();
        }

        protected async Task SaveAsync()
        {
            String jsonString = JsonConvert.SerializeObject(properties, Formatting.Indented);
            using StreamWriter streamWriter = new StreamWriter(SETTINGS_FILE_NAME, false, System.Text.Encoding.UTF8);
            await streamWriter.WriteAsync(jsonString);
        }
    }
}
