using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordChannelsBot.CommandManagement.ChannelManagement;
using DiscordChannelsBot.CommandManagement.CommandHandling;
using DiscordChannelsBot.CommandManagement.MessageFormatting;
using DiscordChannelsBot.Configuration;
using DiscordChannelsBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordChannelsBot;

internal class Program
{
    public static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    private static async Task MainAsync(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();
        await using var serviceScope = host.Services.CreateAsyncScope();

        var serviceProvider = serviceScope.ServiceProvider;
        var bot = serviceProvider.GetRequiredService<DiscordBot>();

        await StartBot(bot);
    }

    private static async Task StartBot(DiscordBot bot)
    {
        await bot.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(ConfigureLogging)
            .ConfigureServices(ConfigureServices);
    }

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddConsole();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        AddDbContext(services);

        services.Configure<DiscordBotConfiguration>(context.Configuration.GetSection("DiscordBot"))
            .AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            }))
            .AddSingleton<CommandService>()
            .AddSingleton<IDiscordBotConfigurationService, DiscordBotFileBasedConfigurationService>()
            .AddSingleton<ICommandHandlingService, CommandHandlingService>()
            .AddSingleton<IVoiceChannelManagementService, VoiceChannelManagementService>()
            .AddSingleton<IMessageFormattingService, MessageFormattingService>()
            .AddSingleton<DiscordBot>();
    }

    private static void AddDbContext(IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString!)
        );
    }
}