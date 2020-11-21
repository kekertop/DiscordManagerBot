using Discord;
using Discord.WebSocket;
using DiscordChannelsBot.Core;
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
        protected const string USERS_BEGIN_CONTENT = "<@!" ;
        protected const string ROLES_BEGIN_CONTENT = "<@&";
        protected const string END_CONTENT = ">";
        protected const string ROLES_REGEX = @"\" + ROLES_BEGIN_CONTENT + @".*?\" + END_CONTENT;
        protected const string USERS_REGEX = @"\" + USERS_BEGIN_CONTENT + @".*?\" + END_CONTENT;
        public MessageFormattingService(IServiceProvider serviceProvider)
        {
            discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
        }

        public GuildGroupsContext GetGuildGroupsContextFromMessage(IGuild guild, ref string message)
        {
            GuildGroupsContext guildGroupsContext = new GuildGroupsContext()
            {
                Roles = GetRolesFromMessage(guild, ref message),
                Users = GetUsersFromMessage(guild, ref message)
            };
            message = message.Trim();
            return guildGroupsContext;
        }

        protected ulong getIdFromString(string idString, IEnumerable<string> garbageStrings)
        {
            string finalString = idString;
            foreach (var garbageString in garbageStrings)
            {
                finalString = finalString.Replace(garbageString, "");
            }
            return ulong.Parse(finalString);
        }

        protected IEnumerable<IRole> GetRolesFromMessage(IGuild guild, ref string message)
        {
            List<IRole> rolesList = new();
            Regex rolesRegex = new Regex(ROLES_REGEX);
            MatchCollection rolesMatches = rolesRegex.Matches(message);
            foreach (Match match in rolesMatches)
            {
                string stringResult = match.Value;
                message = message.Replace(stringResult, "");
                ulong roleId = getIdFromString(stringResult, new string[] { ROLES_BEGIN_CONTENT, END_CONTENT });
                IRole role = guild.GetRole(roleId);
                if (role != null)
                {
                    rolesList.Add(role);
                }
            }
            return rolesList;
        }

        protected IEnumerable<IUser> GetUsersFromMessage(IGuild guild, ref string message)
        {
            List<IUser> usersList = new();
            Regex rolesRegex = new Regex(USERS_REGEX);
            MatchCollection rolesMatches = rolesRegex.Matches(message);
            foreach (Match match in rolesMatches)
            {
                string stringResult = match.Value;
                message = message.Replace(stringResult, "");
                ulong userId = getIdFromString(stringResult, new string[] { USERS_BEGIN_CONTENT, END_CONTENT });
                IUser user = guild.GetUserAsync(userId).GetAwaiter().GetResult();
                if (user != null)
                {
                    usersList.Add(user);
                }
            }
            return usersList;
        }
    }
}
