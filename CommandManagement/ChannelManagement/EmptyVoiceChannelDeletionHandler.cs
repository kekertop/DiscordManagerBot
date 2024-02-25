using Discord.WebSocket;

namespace DiscordChannelsBot.CommandManagement.ChannelManagement;

public class EmptyVoiceChannelDeletionHandler
{
    private readonly DiscordSocketClient _discordClient;

    private readonly Dictionary<ulong, CancellationTokenSource>
        _voiceChannelDeletionCheckCancellationTokenSourcesDictionary = new();

    public EmptyVoiceChannelDeletionHandler(DiscordSocketClient discordClient)
    {
        _discordClient = discordClient;
    }

    public async void RunDeletionCheckAsync(ulong voiceChannelId)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _voiceChannelDeletionCheckCancellationTokenSourcesDictionary.Add(voiceChannelId, cancellationTokenSource);

        try
        {
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);

            var voiceChannel = (SocketVoiceChannel) _discordClient.GetChannel(voiceChannelId);
            if (voiceChannel != null && voiceChannel.ConnectedUsers.Count == 0)
            {
                await voiceChannel.DeleteAsync();
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    public void CancelDeletionCheckIfExists(ulong voiceChannelId)
    {
        if (_voiceChannelDeletionCheckCancellationTokenSourcesDictionary.ContainsKey(voiceChannelId))
        {
            _voiceChannelDeletionCheckCancellationTokenSourcesDictionary[voiceChannelId].Cancel();
            _voiceChannelDeletionCheckCancellationTokenSourcesDictionary.Remove(voiceChannelId);
        }
    }
}