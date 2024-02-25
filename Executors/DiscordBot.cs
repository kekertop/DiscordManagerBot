using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordChannelsBot.CommandManagement.CommandHandling;
using DiscordChannelsBot.CommandManagement.Listeners;
using DiscordChannelsBot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordChannelsBot.Executors;

public class DiscordBot
{
    private readonly DiscordSocketClient _bot;
    private readonly ICommandHandlingService _commandHandlingService;
    private readonly DiscordBotConfiguration _configuration;
    private readonly InteractionService _interactionService;
    private readonly ILogger<DiscordBot> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IVoiceChannelUpdatedListener> _voiceChannelUpdatedListeners;

    public DiscordBot(IServiceProvider serviceProvider,
        IOptions<DiscordBotConfiguration> configuration,
        DiscordSocketClient bot,
        ILogger<DiscordBot> logger,
        ICommandHandlingService commandHandlingService,
        InteractionService interactionService,
        IEnumerable<IVoiceChannelUpdatedListener> voiceChannelUpdatedListeners)
    {
        _configuration = configuration.Value;
        _bot = bot;
        _logger = logger;
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
        _commandHandlingService = commandHandlingService;
        _voiceChannelUpdatedListeners = voiceChannelUpdatedListeners;
    }

    public async Task StartAsync()
    {
        RegisterLogs();
        RegisterListeners();
        await RegisterInteractionsAsync();
        await _bot.LoginAsync(TokenType.Bot, _configuration.Token);
        await _bot.StartAsync();

        _bot.Ready += () => _interactionService.RegisterCommandsGloballyAsync();
    }

    private async Task RegisterInteractionsAsync()
    {
        _bot.InteractionCreated += _commandHandlingService.HandleMessageReceivedAsync;

        _interactionService.InteractionExecuted += _commandHandlingService.HandleCommandExecutedAsync;

        await _interactionService.AddModuleAsync<ChannelsManagementModule>(_serviceProvider);
    }

    private void RegisterListeners()
    {
        foreach (var listener in _voiceChannelUpdatedListeners)
        {
            _bot.UserVoiceStateUpdated += listener.UserVoiceStateUpdatedHandleAsync;
        }
    }

    private void RegisterLogs()
    {
        _bot.Log += Log;
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