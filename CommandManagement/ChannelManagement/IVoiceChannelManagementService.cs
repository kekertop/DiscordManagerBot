using Discord;
using DiscordChannelsBot.Models;

namespace DiscordChannelsBot.CommandManagement.ChannelManagement;

public interface IVoiceChannelManagementService
{
    Task<IVoiceChannel> CreateVoiceChannelAsync(IGuild guild, string name, GuildGroupsContext guildGroupsContext)
    {
        return CreateVoiceChannelAsync(guild, name, guildGroupsContext, true);
    }

    Task<IVoiceChannel> CreateVoiceChannelAsync(IGuild guild, string name, GuildGroupsContext guildGroupsContext,
        bool runDeletionCheck);
}