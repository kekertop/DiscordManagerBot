using Discord.WebSocket;
using DiscordChannelsBot.CommandManagement.ChannelManagement;
using DiscordChannelsBot.Configuration;

namespace DiscordChannelsBot.CommandManagement.Listeners;

public class CreatorVoiceChannelUpdatedListener : IVoiceChannelUpdatedListener
{
    private readonly IDiscordBotConfigurationService _discordBotConfigurationService;
    private readonly IVoiceChannelManagementService _voiceChannelManagementService;

    public CreatorVoiceChannelUpdatedListener(
        IVoiceChannelManagementService voiceChannelManagementService,
        IDiscordBotConfigurationService discordBotConfigurationService)
    {
        _voiceChannelManagementService = voiceChannelManagementService;
        _discordBotConfigurationService = discordBotConfigurationService;
    }

    public async Task UserVoiceStateUpdatedHandleAsync(SocketUser user, SocketVoiceState originalState,
        SocketVoiceState updatedState)
    {
        if (updatedState.VoiceChannel == null)
        {
            return;
        }

        var guild = updatedState.VoiceChannel.Guild;
        var config = await _discordBotConfigurationService.GetGuildConfigurationAsync(guild.Id);
        var creatorVoiceChannelId = config.CreatorVoiceChannelId;
        var voiceChannelId = updatedState.VoiceChannel.Id;

        if (voiceChannelId != creatorVoiceChannelId)
        {
            return;
        }

        var channel =
            await _voiceChannelManagementService.CreateVoiceChannelAsync(guild, config.AutoVoiceChannelName, null);
        var guildUser = guild.Users.First(guildUser => guildUser.Id == user.Id);

        await guild.MoveAsync(guildUser, channel);
    }
}