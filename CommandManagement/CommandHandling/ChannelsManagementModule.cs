using Discord;
using Discord.Interactions;
using DiscordChannelsBot.CommandManagement.ChannelManagement;
using DiscordChannelsBot.Configuration;
using DiscordChannelsBot.Models;

namespace DiscordChannelsBot.CommandManagement.CommandHandling;

public class ChannelsManagementModule : InteractionModuleBase<SocketInteractionContext>
{
    public IDiscordBotConfigurationService DiscordBotConfigurationService { get; init; }
    public IVoiceChannelManagementService VoiceChannelManagementService { get; init; }

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("voice", "Create a voice channel")]
    [RequireContext(ContextType.Guild)]
    public async Task CreateVoiceChannel([Summary("Channel")] string channel,
        [Summary("Permission1", "A person or role that can will able to join the channel")]
        IMentionable firstPermission = null,
        [Summary("Permission2", "A person or role that can will able to join the channel")]
        IMentionable secondPermission = null,
        [Summary("Permission3", "A person or role that can will able to join the channel")]
        IMentionable thirdPermission = null,
        [Summary("Permission4", "A person or role that can will able to join the channel")]
        IMentionable fourthPermission = null,
        [Summary("Permission5", "A person or role that can will able to join the channel")]
        IMentionable fifthPermission = null)
    {
        await CreateVoiceChannel(channel,
            new List<IMentionable>
                    {firstPermission, secondPermission, thirdPermission, fourthPermission, fifthPermission}
                .Where(e => e != null).ToList());
    }

    private async Task CreateVoiceChannel(string channel, List<IMentionable> permissions)
    {
        await Context.Guild.DownloadUsersAsync();

        var guildRolesAndUsers = new GuildGroupsContext
        {
            CurrentUser = Context.User,
            Roles = permissions.OfType<IRole>(),
            Users = permissions.OfType<IUser>()
        };

        await VoiceChannelManagementService.CreateVoiceChannelAsync(Context.Guild, channel, guildRolesAndUsers);

        await Context.Interaction.RespondAsync($"{Context.User.Mention} добавил канал под названием **{channel}**.");
    }

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("voice-category", "Set a guild category where voice channels will be created")]
    [RequireContext(ContextType.Guild)]
    public async Task SetVoiceCategory([Summary("Category")] string category)
    {
        var guildConfiguration = await DiscordBotConfigurationService.GetGuildConfigurationAsync(Context.Guild.Id);

        if (guildConfiguration == null)
        {
            guildConfiguration = new DiscordGuildConfiguration
            {
                Id = Context.Guild.Id,
                VoiceChannelCreationCategory = category
            };

            await DiscordBotConfigurationService.SaveAsync(guildConfiguration);
        }
        else
        {
            guildConfiguration.VoiceChannelCreationCategory = category;

            await DiscordBotConfigurationService.UpdateAsync(guildConfiguration);
        }

        await Context.Interaction.RespondAsync(
            $"Теперь голосовые каналы будут создаваться в категории **{category}**.", ephemeral: true);
    }

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("voice-creator-channel", "Set a channel that creates another voice channel when user enters in")]
    [RequireContext(ContextType.Guild)]
    public async Task SetVoiceCreatorChannel([Summary("Channel")] string channel)
    {
        var guildConfiguration = await DiscordBotConfigurationService.GetGuildConfigurationAsync(Context.Guild.Id);
        var creatorVoiceChannel =
            await VoiceChannelManagementService.CreateVoiceChannelAsync(Context.Guild, channel, null, false);

        if (guildConfiguration == null)
        {
            guildConfiguration = new DiscordGuildConfiguration
            {
                Id = Context.Guild.Id,
                CreatorVoiceChannelId = creatorVoiceChannel.Id
            };

            await DiscordBotConfigurationService.SaveAsync(guildConfiguration);
        }
        else
        {
            guildConfiguration.CreatorVoiceChannelId = creatorVoiceChannel.Id;

            await DiscordBotConfigurationService.UpdateAsync(guildConfiguration);
        }

        await Context.Interaction.RespondAsync(
            $"Теперь канал **{channel}** будет создавать новые каналы.", ephemeral: true);
    }

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("auto-voice-channel-name", "Set a name for automatically created voice channels")]
    [RequireContext(ContextType.Guild)]
    public async Task SetAutoVoiceChannelName([Summary("Channel")] string channel)
    {
        var guildConfiguration = await DiscordBotConfigurationService.GetGuildConfigurationAsync(Context.Guild.Id);

        if (guildConfiguration == null)
        {
            guildConfiguration = new DiscordGuildConfiguration
            {
                Id = Context.Guild.Id,
                AutoVoiceChannelName = channel
            };

            await DiscordBotConfigurationService.SaveAsync(guildConfiguration);
        }
        else
        {
            guildConfiguration.AutoVoiceChannelName = channel;

            await DiscordBotConfigurationService.UpdateAsync(guildConfiguration);
        }

        await Context.Interaction.RespondAsync(
            $"Теперь автоматические каналы будут называться **{channel}**.", ephemeral: true);
    }
}