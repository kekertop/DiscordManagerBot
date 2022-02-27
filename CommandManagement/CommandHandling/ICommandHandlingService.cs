using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordChannelsBot.CommandManagement.CommandHandling;

public interface ICommandHandlingService
{
    Task HandleCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result);

    Task HandleMessageReceivedAsync(SocketMessage socketMessage, DiscordSocketClient discordClient,
        CommandService commandService);
}