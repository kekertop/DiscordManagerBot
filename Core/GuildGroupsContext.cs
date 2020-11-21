using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordChannelsBot.Core
{
    public class GuildGroupsContext
    {
        public IEnumerable<IRole> Roles { get; set; }
        public IEnumerable<IUser> Users { get; set; }    
    }
}
