using Discord;
using Discord.WebSocket;
using DiscordChannelsBot.Configuration;
using Microsoft.Extensions.Logging;

namespace DiscordChannelsBot;

public class DiscordBot
{
    private readonly DiscordSocketClient _bot;
    private readonly DiscordBotConfiguration _configuration;
    private readonly ILogger _logger;

    public DiscordBot(DiscordBotConfiguration configuration, DiscordSocketClient bot, ILogger logger)
    {
        _configuration = configuration;
        _bot = bot;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        _bot.Log += Log;

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