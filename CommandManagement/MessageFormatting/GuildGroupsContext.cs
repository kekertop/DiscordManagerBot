using System.Collections.Generic;
using Discord;

namespace DiscordChannelsBot.CommandManagement.MessageFormatting
{
    public class GuildGroupsContext
    {
        public IUser CurrentUser { get; init; }
        public IEnumerable<IRole> Roles { get; init; }
        public IEnumerable<IUser> Users { get; init; }
    }
}