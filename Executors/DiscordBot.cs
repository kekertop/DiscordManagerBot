using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordChannelsBot.CommandManagement.CommandHandling;
using DiscordChannelsBot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordChannelsBot;

public class DiscordBot : IAsyncExecutor
{
    private readonly DiscordSocketClient _bot;
    private readonly DiscordBotConfiguration _configuration;
    private readonly InteractionService _interactionService;
    private readonly ILogger<DiscordBot> _logger;

    public DiscordBot(IServiceProvider serviceProvider,
        IOptions<DiscordBotConfiguration> configuration,
        DiscordSocketClient bot,
        ILogger<DiscordBot> logger,
        ICommandHandlingService commandHandlingService,
        InteractionService interactionService)
    {
        _configuration = configuration.Value;
        _bot = bot;
        _logger = logger;
        _interactionService = interactionService;

        _bot.Log += Log;

        _bot.InteractionCreated += commandHandlingService.HandleMessageReceivedAsync;

        _interactionService.AddModuleAsync<ChannelsManagementModule>(serviceProvider).ConfigureAwait(false).GetAwaiter()
            .GetResult();
        _interactionService.InteractionExecuted += commandHandlingService.HandleCommandExecutedAsync;
    }

    public async Task StartAsync()
    {
        await _bot.LoginAsync(TokenType.Bot, _configuration.Token);
        await _bot.StartAsync();

        _bot.Ready += async () => await _interactionService.RegisterCommandsGloballyAsync();
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