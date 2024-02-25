﻿using DiscordChannelsBot.Models;

namespace DiscordChannelsBot.Configuration;

public class DiscordBotFileBasedConfigurationService : IDiscordBotConfigurationService
{
    private readonly ApplicationDbContext _applicationDbContext;

    public DiscordBotFileBasedConfigurationService(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async ValueTask<DiscordGuildConfiguration> GetGuildConfigurationAsync(ulong guildId)
    {
        return await _applicationDbContext.GuildConfigurations
            .FindAsync(guildId);
    }

    public async Task UpdateAsync(DiscordGuildConfiguration discordGuildConfiguration)
    {
        _applicationDbContext.GuildConfigurations.Update(discordGuildConfiguration);
        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task SaveAsync(DiscordGuildConfiguration discordGuildConfiguration)
    {
        await _applicationDbContext.GuildConfigurations.AddAsync(discordGuildConfiguration);
        await _applicationDbContext.SaveChangesAsync();
    }
}