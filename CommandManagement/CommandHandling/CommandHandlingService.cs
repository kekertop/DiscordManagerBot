using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordChannelsBot.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordChannelsBot.CommandManagement.CommandHandling;

internal class CommandHandlingService : ICommandHandlingService
{
    private readonly CommandService _commandService;
    private readonly DiscordBotConfiguration _discordBotConfiguration;
    private readonly DiscordSocketClient _discordClient;
    private readonly IServiceProvider _serviceProvider;

    public CommandHandlingService(IServiceProvider serviceProvider, DiscordBotConfiguration discordBotConfiguration)
    {
        _commandService = serviceProvider.GetRequiredService<CommandService>();
        _discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
        _serviceProvider = serviceProvider;
        _discordBotConfiguration = discordBotConfiguration;

        _discordClient.MessageReceived += MessageReceivedAsync;
        _commandService.CommandExecuted += CommandExecutedAsync;
        _commandService.AddModuleAsync<ChannelsManagementModule>(_serviceProvider).ConfigureAwait(false).GetAwaiter()
            .GetResult();
    }

    private async Task MessageReceivedAsync(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage { Source: MessageSource.User } userMessage) return;

        var prefixEndIndex = 0;
        if (userMessage.HasStringPrefix(_discordBotConfiguration.Prefix,
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