using Discord;
using Discord.Commands;
using DiscordChannelsBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordChannelsBot.Commands
{
    public class ChannelsManagementModule : ModuleBase<SocketCommandContext>
    {
        public DiscordBotConfigurationService DiscordBotConfigurationService { get; set; }
        public VoiceChannelManagementService VoiceChannelManagementService { get; set; }
        public MessageFormattingService MessageFormattingService { get; set; }
        [Command("voice")]
        [RequireContext(ContextType.Guild)]
        public async Task CreateVoiceChannel([Remainder] string paramsMessage)
        {
            if (paramsMessage != null && paramsMessage.Length > 0)
            {
                IEnumerable<IRole> roles = MessageFormattingService.GetRolesFromMessage(Context.Guild, ref paramsMessage);
                await VoiceChannelManagementService.CreateVoiceChannelAsync(Context.Guild, paramsMessage, roles);
                await Context.Channel.SendMessageAsync($"Добавил канал под названием **{paramsMessage}**.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Вы не написали название канала, который хотите создать!");
            }
        }
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels)]
        [Command("voicecategory")]
        [RequireContext(ContextType.Guild)]
        public async Task SetVoiceCategory([Remainder] string name)
        {
            if (name != null && name.Length > 0)
            {
                DiscordBotConfigurationService.DiscordBotConfiguration.SetCategory(Context.Guild.Id, name);
                await DiscordBotConfigurationService.CommitCurrentState();
                await Context.Channel.SendMessageAsync($"Теперь голосовые каналы будут создаваться в категории **{name}**.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Вы не написали название категории, в которую вы хотите добавлять каналы!");
            }
        }
    }
}
