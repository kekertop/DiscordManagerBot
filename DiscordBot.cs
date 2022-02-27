using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordChannelsBot.CommandManagement.CommandHandling;
using DiscordChannelsBot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordChannelsBot;

public class DiscordBot
{
    private readonly DiscordSocketClient _bot;
    private readonly DiscordBotConfiguration _configuration;
    private readonly ILogger<DiscordBot> _logger;

    public DiscordBot(IServiceProvider serviceProvider, IOptions<DiscordBotConfiguration> configuration,
        CommandService commandService, DiscordSocketClient bot, ILogger<DiscordBot> logger,
        ICommandHandlingService commandHandlingService)
    {
        _configuration = configuration.Value;
        _bot = bot;
        _logger = logger;

        _bot.Log += Log;
        _bot.MessageReceived += message =>
            commandHandlingService.HandleMessageReceivedAsync(message, _bot, commandService);

        commandService.CommandExecuted += commandHandlingService.HandleCommandExecutedAsync;
        commandService.AddModuleAsync<ChannelsManagementModule>(serviceProvider).ConfigureAwait(false).GetAwaiter()
            .GetResult();
    }

    public async Task StartAsync()
    {
        await _bot.LoginAsync(TokenType.Bot, _configuration.Token);
        await _bot.StartAsync();
    }

    private Task Log(LogMessage log)
    {
        _logger.Log(ConvertLogLevel(log.Severity), log.Exception, log.Message);

        return Task.CompletedTask;
    }

    private LogLevel ConvertLogLevel(LogSeverity logSeverity)
    {
        return logSeverity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => throw new ArgumentOutOfRangeException(nameof(logSeverity), logSeverity, null)
        };
    }
}