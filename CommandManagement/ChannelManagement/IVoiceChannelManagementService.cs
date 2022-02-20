using Discord;
using DiscordChannelsBot.CommandManagement.MessageFormatting;

namespace DiscordChannelsBot.CommandManagement.ChannelManagement;

public interface IVoiceChannelManagementService
{
    Task CreateVoiceChannelAsync(IGuild guild, string name, GuildGroupsContext guildGroupsContext);
}