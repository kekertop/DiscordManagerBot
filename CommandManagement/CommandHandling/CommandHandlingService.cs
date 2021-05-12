using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordChannelsBot.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordChannelsBot.CommandManagement.CommandHandling
{
    internal class CommandHandlingService
    {
        private readonly CommandService _commandService;
        private readonly IDiscordBotConfigurationService _discordBotConfigurationService;
        private readonly DiscordSocketClient _discordClient;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandlingService(IServiceProvider serviceProvider)
        {
            _commandService = serviceProvider.GetRequiredService<CommandService>();
            _discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
            _discordBotConfigurationService = serviceProvider.GetRequiredService<IDiscordBotConfigurationService>();
            _discordClient.MessageReceived += MessageReceivedAsync;
            _commandService.CommandExecuted += CommandExecutedAsync;
            _serviceProvider = serviceProvider;
            _commandService.AddModuleAsync<ChannelsManagementModule>(_serviceProvider).Wait();
        }

        private async Task MessageReceivedAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage)) return;
            if (socketMessage.Source != MessageSource.User) return;
            var userMessage = (SocketUserMessage) socketMessage;
            var prefixEndIndex = 0;
            if (userMessage.HasStringPrefix(_discordBotConfigurationService.GetBotConfiguration().Prefix,
                ref prefixEndIndex))
            {
                SocketCommandContext commandContext = new(_discordClient, userMessage);
                await _commandService.ExecuteAsync(commandContext, prefixEndIndex, _serviceProvider);
            }
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;
            if (result.IsSuccess)
                return;
            switch (result.Error)
            {
                case CommandError.BadArgCount:
                    await context.Channel.SendMessageAsync(
                        "Произошла ошибка: вы написали слишком мало аргументов в команде.");
                    return;
                case CommandError.UnknownCommand:
                    await context.Channel.SendMessageAsync("Произошла ошибка: команда не распознана.");
                    return;
                case CommandError.ParseFailed:
                    await context.Channel.SendMessageAsync("Произошла ошибка: команда не распознана.");
                    return;
                case CommandError.Exception:
                    await context.Channel.SendMessageAsync($"Произошла ошибка: {result}");
                    return;
                case CommandError.Unsuccessful:
                    await context.Channel.SendMessageAsync("Произошла ошибка: команда не выполнена.");
                    return;
            }
        }
    }
}