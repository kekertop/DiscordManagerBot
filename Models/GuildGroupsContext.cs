using Discord;

namespace DiscordChannelsBot.Models;

public class GuildGroupsContext
{
    public IUser CurrentUser { get; init; }
    public IEnumerable<IRole> Roles { get; init; }
    public IEnumerable<IUser> Users { get; init; }
}