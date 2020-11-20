using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordChannelsBot.Services
{
    public class MessageFormattingService
    {
        protected DiscordSocketClient discordClient;
        public MessageFormattingService(IServiceProvider serviceProvider)
        {
            discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
        }

        public IEnumerable<IRole> GetRolesFromMessage(IGuild guild, ref string message)
        {
            List<IRole> rolesList = new();
            Regex regex = new Regex(@"\<@&.*?\>");
            MatchCollection matches = regex.Matches(message);
            foreach (Match match in matches)
            {
                string stringResult = match.Value;
                message = message.Replace(stringResult, "");
                ulong roleId = ulong.Parse(stringResult.Replace("<@&", "").Replace(">", ""));
                IRole role = guild.GetRole(roleId);
                if (role != null)
                {
                    rolesList.Add(role);
                }
            }
            return rolesList;
        }
    }
}
