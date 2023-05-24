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
}