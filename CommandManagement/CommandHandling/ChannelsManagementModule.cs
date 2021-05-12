using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordChannelsBot.CommandManagement.ChannelManagement;
using DiscordChannelsBot.CommandManagement.MessageFormatting;
using DiscordChannelsBot.Configuration;

namespace DiscordChannelsBot.CommandManagement.CommandHandling
{
    public class ChannelsManagementModule : ModuleBase<SocketCommandContext>
    {
        public IDiscordBotConfigurationService DiscordBotConfigurationService { get; init; }
        public VoiceChannelManagementService VoiceChannelManagementService { get; init; }
        public MessageFormattingService MessageFormattingService { get; init; }

        [Command("voice")]
        [RequireContext(ContextType.Guild)]
        public async Task CreateVoiceChannel([Remainder] string paramsMessage)
        {
            await Context.Guild.DownloadUsersAsync();
            if (paramsMessage != null && paramsMessage.Length > 0)
            {
                var guildRolesAndUsers =
                    MessageFormattingService.GetGuildGroupsContextFromMessage(Context.Guild, Context.User,
                        ref paramsMessage);
                if (paramsMessage != null && paramsMessage.Length > 0) //if voice channel is not null 
                {
                    await VoiceChannelManagementService.CreateVoiceChannelAsync(Context.Guild, paramsMessage,
                        guildRolesAndUsers);
                    await Context.Channel.SendMessageAsync($"Добавил канал под названием **{paramsMessage}**.");
                    return;
                }
            }

            await Context.Channel.SendMessageAsync("Вы не написали название канала, который хотите создать!");
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("voicecategory")]
        [RequireContext(ContextType.Guild)]
        public async Task SetVoiceCategory([Remainder] string name)
        {
            if (name != null && name.Length > 0)
            {
                var guildConfiguration = DiscordBotConfigurationService.GetGuildConfiguration(Context.Guild.Id);
                guildConfiguration.VoiceChannelCreationCategory = name;
                await DiscordBotConfigurationService.SaveGuildConfiguration(guildConfiguration);
                await Context.Channel.SendMessageAsync(
                    $"Теперь голосовые каналы будут создаваться в категории **{name}**.");
            }
            else
            {
                await Context.Channel.SendMessageAsync(
                    "Вы не написали название категории, в которую вы хотите добавлять каналы!");
            }
        }
    }
}