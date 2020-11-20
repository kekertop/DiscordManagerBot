using Discord.Commands;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Discord.WebSocket;
using Discord;
using DiscordChannelsBot.Commands;
using DiscordChannelsBot.AppConfiguration;

namespace DiscordChannelsBot.Services
{
    class CommandHandlingService
    {
        protected readonly DiscordBotConfigurationService discordBotConfiguration;
        protected readonly CommandService commandService;
        protected readonly IServiceProvider serviceProvider;
        protected readonly DiscordSocketClient discordClient;

        public CommandHandlingService(IServiceProvider serviceProvider)
        {
            commandService = serviceProvider.GetRequiredService<CommandService>();
            discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
            discordBotConfiguration = serviceProvider.GetRequiredService<DiscordBotConfigurationService>();
            discordClient.MessageReceived += MessageReceivedAsync;
            commandService.CommandExecuted += CommandExecutedAsync;
            this.serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync()
        {
            await commandService.AddModuleAsync<ChannelsManagementModule>(serviceProvider);
        }

        public async Task MessageReceivedAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage)) return;
            if (socketMessage.Source != Discord.MessageSource.User) return;
            SocketUserMessage userMessage = (SocketUserMessage)socketMessage;
            int prefixEndIndex = 0;
            if (userMessage.HasStringPrefix(discordBotConfiguration.DiscordBotConfiguration.DiscordPrefix, ref prefixEndIndex))
            {
                SocketCommandContext commandContext = new(discordClient, userMessage);
                await commandService.ExecuteAsync(commandContext, prefixEndIndex, serviceProvider);
            }
            return;
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;
            if (result.IsSuccess)
                return;
            await context.Channel.SendMessageAsync($"Произошла ошибка: {result}");
        }
    }
}
