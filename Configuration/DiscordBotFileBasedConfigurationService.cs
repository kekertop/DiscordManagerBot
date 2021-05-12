using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordChannelsBot.Configuration
{
    public class DiscordBotFileBasedConfigurationService : IDiscordBotConfigurationService
    {
        private const string BOT_CONFIGURATION_PROPERTY = "bot";
        private const string GUILD_CONFIGURATION_PROPERTY = "guilds";
        private const string SETTINGS_FILE_NAME = "botconfig.json";

        private Dictionary<string, JContainer> _configuration;
        private FileSystemWatcher _configurationFileWatcher;

        public DiscordBotFileBasedConfigurationService()
        {
            _configuration = GetCurrentConfiguration().Result;
            _configurationFileWatcher = InitializeConfigurationFileWatcher();
        }

        public DiscordBotConfiguration GetBotConfiguration()
        {
            return _configuration[BOT_CONFIGURATION_PROPERTY].ToObject<DiscordBotConfiguration>();
        }

        public DiscordGuildConfiguration GetGuildConfiguration(ulong guildId)
        {
            return (_configuration[GUILD_CONFIGURATION_PROPERTY]
                    .ToObject<List<DiscordGuildConfiguration>>() ?? new List<DiscordGuildConfiguration>())
                .FirstOrDefault(configuration => configuration.GuildId == guildId) ?? new DiscordGuildConfiguration
                {
                    GuildId = guildId
                };
        }

        public async Task SaveGuildConfiguration(DiscordGuildConfiguration configuration)
        {
            var currentGuildConfigurations =
                _configuration[GUILD_CONFIGURATION_PROPERTY].ToObject<List<DiscordGuildConfiguration>>() ??
                new List<DiscordGuildConfiguration>();
            var equalConfiguration =
                currentGuildConfigurations.FirstOrDefault(currentConfiguration =>
                    currentConfiguration.Equals(configuration));
            if (equalConfiguration != null)
                equalConfiguration.VoiceChannelCreationCategory = configuration.VoiceChannelCreationCategory;
            else
                currentGuildConfigurations.Add(configuration);

            _configuration[GUILD_CONFIGURATION_PROPERTY] = JArray.FromObject(currentGuildConfigurations);
            var jsonString = JsonConvert.SerializeObject(_configuration, Formatting.Indented);
            await using var streamWriter = new StreamWriter(SETTINGS_FILE_NAME, false, Encoding.UTF8);
            await streamWriter.WriteAsync(jsonString);
        }

        private FileSystemWatcher InitializeConfigurationFileWatcher()
        {
            var watcher = new FileSystemWatcher
            {
                Path = Directory.GetCurrentDirectory(),
                Filter = SETTINGS_FILE_NAME
            };
            watcher.Changed += ConfigurationUpdateHandler;
            return watcher;
        }

        private async Task<Dictionary<string, JContainer>> GetCurrentConfiguration()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, JContainer>>(
                await File.ReadAllTextAsync(SETTINGS_FILE_NAME));
        }

        private async void ConfigurationUpdateHandler(object sender, FileSystemEventArgs e)
        {
            _configuration = await GetCurrentConfiguration();
        }
    }
}