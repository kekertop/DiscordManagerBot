using Discord.WebSocket;

namespace DiscordChannelsBot.CommandManagement.Listeners;

public interface IVoiceChannelUpdatedListener
{
    Task UserVoiceStateUpdatedHandleAsync(SocketUser user, SocketVoiceState originalState,
        SocketVoiceState updatedState);
}