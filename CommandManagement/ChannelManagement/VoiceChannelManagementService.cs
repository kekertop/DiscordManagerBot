using Discord;
using DiscordChannelsBot.Common;
using DiscordChannelsBot.Configuration;
using DiscordChannelsBot.Models;

namespace DiscordChannelsBot.CommandManagement.ChannelManagement;

public class VoiceChannelManagementService : IVoiceChannelManagementService
{
    private readonly EmptyVoiceChannelDeletionHandler _deletionHandler;
    private readonly IDiscordBotConfigurationService _discordBotConfigurationService;

    public VoiceChannelManagementService(IDiscordBotConfigurationService discordBotConfigurationService,
        EmptyVoiceChannelDeletionHandler deletionHandler)
    {
        _discordBotConfigurationService = discordBotConfigurationService;
        _deletionHandler = deletionHandler;
    }

    public async Task<IVoiceChannel> CreateVoiceChannelAsync(IGuild guild, string name,
        GuildGroupsContext guildGroupsContext, bool runDeletionCheck)
    {
        var guildConfiguration = await _discordBotConfigurationService.GetGuildConfigurationAsync(guild.Id);
        if (guildConfiguration == null)
        {
            throw new ArgumentException("Не указана категория для создания голосовых каналов!");
        }

        var categoryChannel =
            await DiscordBotUtils.GetCategoryAsync(guild, guildConfiguration.VoiceChannelCreationCategory);

        Action<VoiceChannelProperties> voiceChannelProperties = _ => { };
        if (categoryChannel != null)
        {
            voiceChannelProperties += channel => channel.CategoryId = categoryChannel.Id;
        }

        var voiceChannel = await guild.CreateVoiceChannelAsync(name, voiceChannelProperties);

        if (guildGroupsContext != null)
        {
            if (guildGroupsContext.Roles != null && guildGroupsContext.Roles.Any() ||
                guildGroupsContext.Users != null && guildGroupsContext.Users.Any())
            {
                await AllowOnlyRolesAsync(guild, voiceChannel, guildGroupsContext);
            }
        }

        if (runDeletionCheck)
        {
            _deletionHandler.RunDeletionCheckAsync(voiceChannel.Id);
        }

        return voiceChannel;
    }

    private async Task AllowOnlyRolesAsync(IGuild guild, IVoiceChannel voiceChannel,
        GuildGroupsContext guildGroupsContext)
    {
        var rolePermissions = new OverwritePermissions().Modify(viewChannel: PermValue.Allow,
            connect: PermValue.Allow,
            speak: PermValue.Allow);

        var denyPermissions = new OverwritePermissions().Modify(viewChannel: PermValue.Deny,
            connect: PermValue.Deny,
            speak: PermValue.Deny);

        await voiceChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, denyPermissions);

        await voiceChannel.AddPermissionOverwriteAsync(guildGroupsContext.CurrentUser, rolePermissions);

        if (guildGroupsContext.Roles != null)
        {
            foreach (var role in guildGroupsContext.Roles)
            {
                await voiceChannel.AddPermissionOverwriteAsync(role, rolePermissions);
            }
        }

        if (guildGroupsContext.Users != null)
        {
            foreach (var user in guildGroupsContext.Users)
            {
                await voiceChannel.AddPermissionOverwriteAsync(user, rolePermissions);
            }
        }
    }
}