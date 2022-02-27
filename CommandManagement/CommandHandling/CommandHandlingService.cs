using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordChannelsBot.Models;
using Microsoft.Extensions.Options;

namespace DiscordChannelsBot.CommandManagement.CommandHandling;

internal class CommandHandlingService : ICommandHandlingService
{
    private readonly DiscordBotConfiguration _discordBotConfiguration;
    private readonly IServiceProvider _serviceProvider;

    public CommandHandlingService(IServiceProvider serviceProvider,
        IOptions<DiscordBotConfiguration> discordBotConfiguration)
    {
        _serviceProvider = serviceProvider;
        _discordBotConfiguration = discordBotConfiguration.Value;
    }

    public async Task HandleCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (!command.IsSpecified)
        {
            return;
        }

        if (result.IsSuccess)
        {
            return;
        }

        switch (result.Error)
        {
            case CommandError.BadArgCount:
                await context.Channel.SendMessageAsync(
                    "Произошла ошибка. Слишком мало аргументов в команде.");
                return;
            case CommandError.UnknownCommand:
            case CommandError.ParseFailed:
                await context.Channel.SendMessageAsync("Произошла ошибка. Команда не распознана.");
                return;
            case CommandError.Exception:
                await context.Channel.SendMessageAsync($"Произошла ошибка. {result.ErrorReason}");
                return;
            case CommandError.Unsuccessful:
                await context.Channel.SendMessageAsync("Произошла ошибка. Команда не выполнена.");
                return;
        }
    }

    public async Task HandleMessageReceivedAsync(SocketMessage socketMessage, DiscordSocketClient discordClient,
        CommandService commandService)
    {
        if (socketMessage is not SocketUserMessage {Source: MessageSource.User} userMessage)
        {
            return;
        }

        var prefixEndIndex = 0;
        if (userMessage.HasStringPrefix(_discordBotConfiguration.Prefix,
                ref prefixEndIndex))
        {
            SocketCommandContext commandContext = new(discordClient, userMessage);
            await commandService.ExecuteAsync(commandContext, prefixEndIndex, _serviceProvider);
        }
    }
}