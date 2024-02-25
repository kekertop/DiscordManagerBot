using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DiscordChannelsBot.CommandManagement.CommandHandling;

public interface ICommandHandlingService
{
    Task HandleCommandExecutedAsync(ICommandInfo command, IInteractionContext context,
        IResult result);

    Task HandleMessageReceivedAsync(SocketInteraction interaction);
}