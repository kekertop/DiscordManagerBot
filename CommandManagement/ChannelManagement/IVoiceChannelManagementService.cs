using Discord;
using DiscordChannelsBot.Models;

namespace DiscordChannelsBot.CommandManagement.ChannelManagement;

public interface IVoiceChannelManagementService
{
    Task CreateVoiceChannelAsync(IGuild guild, string name, GuildGroupsContext guildGroupsContext);
}