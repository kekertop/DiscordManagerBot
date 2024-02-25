using Discord;
using Discord.WebSocket;
using DiscordChannelsBot.CommandManagement.ChannelManagement;
using DiscordChannelsBot.Configuration;

namespace DiscordChannelsBot.CommandManagement.Listeners;

public class VoiceChannelDeletionCheckListener : IVoiceChannelUpdatedListener
{
    private readonly EmptyVoiceChannelDeletionHandler _deletionHandler;
    private readonly IDiscordBotConfigurationService _discordBotConfigurationService;

    public VoiceChannelDeletionCheckListener(
        EmptyVoiceChannelDeletionHandler deletionHandler,
        IDiscordBotConfigurationService discordBotConfigurationService)
    {
        _deletionHandler = deletionHandler;
        _discordBotConfigurationService = discordBotConfigurationService;
    }

    public async Task UserVoiceStateUpdatedHandleAsync(SocketUser user, SocketVoiceState originalState,
        SocketVoiceState updatedState)
    {
        IGuild guild = null;
        if (originalState.VoiceChannel != null)
        {
            guild = originalState.VoiceChannel.Guild;
        }
        else if (updatedState.VoiceChannel != null)
        {
            guild = updatedState.VoiceChannel.Guild;
        }

        var config = await _discordBotConfigurationService.GetGuildConfigurationAsync(guild!.Id);
        var creatorChannelId = config.CreatorVoiceChannelId;

        if (originalState.VoiceChannel != null
            && originalState.VoiceChannel.Id != creatorChannelId)
        {
            var voiceChannel = originalState.VoiceChannel;
            if (voiceChannel.ConnectedUsers.Count == 0)
            {
                _deletionHandler.RunDeletionCheckAsync(voiceChannel.Id);
            }
        }

        if (updatedState.VoiceChannel != null
            && updatedState.VoiceChannel.Id != creatorChannelId)
        {
            var voiceChannel = updatedState.VoiceChannel;
            _deletionHandler.CancelDeletionCheckIfExists(voiceChannel.Id);
        }
    }
}