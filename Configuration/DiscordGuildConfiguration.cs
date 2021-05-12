namespace DiscordChannelsBot.Configuration
{
    public class DiscordGuildConfiguration
    {
        public ulong GuildId { get; init; }

        public string VoiceChannelCreationCategory { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return GuildId == ((DiscordGuildConfiguration) obj).GuildId;
        }
    }
}