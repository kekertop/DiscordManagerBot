using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordChannelsBot.CommandManagement.MessageFormatting
{
    public class MessageFormattingService : IMessageFormattingService
    {
        protected const string USERS_BEGIN_CONTENT = "<@!";
        protected const string ROLES_BEGIN_CONTENT = "<@&";
        protected const string END_CONTENT = ">";
        protected const string ROLES_REGEX = @"\" + ROLES_BEGIN_CONTENT + @".*?\" + END_CONTENT;
        protected const string USERS_REGEX = @"\" + USERS_BEGIN_CONTENT + @".*?\" + END_CONTENT;
        protected DiscordSocketClient discordClient;

        public MessageFormattingService(IServiceProvider serviceProvider)
        {
            discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
        }

        public GuildGroupsContext GetGuildGroupsContextFromMessage(IGuild guild, IUser user, ref string message)
        {
            var guildGroupsContext = new GuildGroupsContext
            {
                CurrentUser = user,
                Roles = GetRolesFromMessage(guild, ref message),
                Users = GetUsersFromMessage(guild, ref message)
            };
            message = message.Trim();
            return guildGroupsContext;
        }

        private ulong getIdFromString(string idString, IEnumerable<string> garbageStrings)
        {
            var finalString = idString;
            foreach (var garbageString in garbageStrings) finalString = finalString.Replace(garbageString, "");
            return ulong.Parse(finalString);
        }

        private IEnumerable<IRole> GetRolesFromMessage(IGuild guild, ref string message)
        {
            List<IRole> rolesList = new();
            var rolesRegex = new Regex(ROLES_REGEX);
            var rolesMatches = rolesRegex.Matches(message);
            foreach (Match match in rolesMatches)
            {
                var stringResult = match.Value;
                message = message.Replace(stringResult, "");
                var roleId = getIdFromString(stringResult, new[] {ROLES_BEGIN_CONTENT, END_CONTENT});
                var role = guild.GetRole(roleId);
                if (role != null) rolesList.Add(role);
            }

            return rolesList;
        }

        private IEnumerable<IUser> GetUsersFromMessage(IGuild guild, ref string message)
        {
            List<IUser> usersList = new();
            var rolesRegex = new Regex(USERS_REGEX);
            var rolesMatches = rolesRegex.Matches(message);
            foreach (Match match in rolesMatches)
            {
                var stringResult = match.Value;
                message = message.Replace(stringResult, "");
                var userId = getIdFromString(stringResult, new[] {USERS_BEGIN_CONTENT, END_CONTENT});
                IUser user = guild.GetUserAsync(userId).GetAwaiter().GetResult();
                if (user != null) usersList.Add(user);
            }

            return usersList;
        }
    }
}