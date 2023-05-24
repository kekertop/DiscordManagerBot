using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordChannelsBot.Models;
using Microsoft.Extensions.Options;

namespace DiscordChannelsBot.CommandManagement.CommandHandling;

internal class CommandHandlingService : ICommandHandlingService
{
    private readonly DiscordSocketClient _bot;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    public CommandHandlingService(IServiceProvider serviceProvider,
        IOptions<DiscordBotConfiguration> discordBotConfiguration, InteractionService interactionService,
        DiscordSocketClient bot)
    {
        _serviceProvider = serviceProvider;
        _interactionService = interactionService;
        _bot = bot;
    }

    public async Task HandleCommandExecutedAsync(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
        {
            return;
        }

        switch (result.Error)
        {
            case InteractionCommandError.BadArgs:
                await context.Interaction.RespondAsync(
                    "Произошла ошибка. Слишком мало аргументов в команде.", ephemeral: true);
                return;
            case InteractionCommandError.Exception:
                await context.Interaction.RespondAsync($"Произошла ошибка. {result.ErrorReason}", ephemeral: true);
                return;
            case InteractionCommandError.Unsuccessful:
                await context.Interaction.RespondAsync("Произошла ошибка. Команда не выполнена.", ephemeral: true);
                return;
            case InteractionCommandError.ParseFailed:
                await context.Interaction.RespondAsync("Произошла ошибка. Команда не распознана.", ephemeral: true);
                return;
            default:
                await context.Interaction.RespondAsync("Произошла ошибка.", ephemeral: true);
                return;
        }
    }

    public async Task HandleMessageReceivedAsync(SocketInteraction interaction)
    {
        await _interactionService.ExecuteCommandAsync(new SocketInteractionContext(_bot, interaction),
            _serviceProvider);
    }
}