﻿using Discord;

namespace DiscordChannelsBot.CommandManagement.MessageFormatting
{
    public interface IMessageFormattingService
    {
        GuildGroupsContext GetGuildGroupsContextFromMessage(IGuild guild, IUser user, ref string message);
    }
}