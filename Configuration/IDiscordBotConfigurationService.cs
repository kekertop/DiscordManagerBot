using DiscordChannelsBot.Models;

namespace DiscordChannelsBot.Configuration;

public interface IDiscordBotConfigurationService
{
    ValueTask<DiscordGuildConfiguration> GetGuildConfigurationAsync(ulong guildId);

    Task UpdateAsync(DiscordGuildConfiguration discordGuildConfiguration);

    Task SaveAsync(DiscordGuildConfiguration discordGuildConfiguration);
}