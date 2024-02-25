namespace DiscordChannelsBot.Models;

public class DiscordGuildConfiguration
{
    public ulong Id { get; init; }

    public string VoiceChannelCreationCategory { get; set; }

    public ulong? CreatorVoiceChannelId { get; set; }

    public string AutoVoiceChannelName { get; set; }
}