using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordChannelsBot.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DiscordChannelsBot.AppConfiguration;
namespace DiscordChannelsBot
{
    class Program
    {
        public static void Main()
            => new Program().MainAsync().GetAwaiter().GetResult();


        public async Task MainAsync()
        {
            using ServiceProvider services = ConfigureServices();
            DiscordBotConfigurationService discordConfigurationService = services.GetRequiredService<DiscordBotConfigurationService>();
            DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += LogAsync;
            await client.LoginAsync(TokenType.Bot, discordConfigurationService.DiscordBotConfiguration.DiscordToken);
            await client.StartAsync();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
            await Task.Delay(Timeout.Infinite);
        }

        protected static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<ConfigurationService>()
                .AddSingleton<DiscordBotConfigurationService>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<VoiceChannelManagementService>()
                .AddSingleton<MessageFormattingService>()
                .BuildServiceProvider();
        }

        protected Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
