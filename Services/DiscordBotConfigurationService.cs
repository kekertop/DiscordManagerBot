using DiscordChannelsBot.AppConfiguration;
using System;
using Microsoft.Extensions.DependencyInjection;
 
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordChannelsBot.Services
{
    public class DiscordBotConfigurationService
    {
        protected const string DISCORD_BOT_CONFIGURATION_PROPERTY = "discordBotConfiguration";
        protected ConfigurationService configurationService;

        public DiscordBotConfiguration DiscordBotConfiguration { get; protected set; }

        public DiscordBotConfigurationService(IServiceProvider serviceProvider)
        {
            configurationService = serviceProvider.GetRequiredService<ConfigurationService>();
            configurationService.ConfigurationUpdated += UpdateDiscordBotConfiguration;
            UpdateDiscordBotConfiguration();
        }

        public async Task CommitCurrentState()
        {
            await configurationService.SetPropertyAsync(DISCORD_BOT_CONFIGURATION_PROPERTY, DiscordBotConfiguration);
        }
        protected void UpdateDiscordBotConfiguration()
        {
            DiscordBotConfiguration = configurationService.GetProperty<DiscordBotConfiguration>(DISCORD_BOT_CONFIGURATION_PROPERTY);
        }
    }
}
