using System.Threading.Tasks;

namespace DiscordChannelsBot.Configuration
{
    public interface IDiscordBotConfigurationService
    {
        DiscordBotConfiguration GetBotConfiguration();

        DiscordGuildConfiguration GetGuildConfiguration(ulong guildId);

        Task SaveGuildConfiguration(DiscordGuildConfiguration configuration);
    }
}