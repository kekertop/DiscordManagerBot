using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordChannelsBot.CommandManagement.ChannelManagement;
using DiscordChannelsBot.CommandManagement.CommandHandling;
using DiscordChannelsBot.CommandManagement.MessageFormatting;
using DiscordChannelsBot.Common;
using DiscordChannelsBot.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordChannelsBot
{
    internal class Program
    {
        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }


        private static async Task MainAsync()
        {
            await using var services = ConfigureServices();
            var discordConfigurationService =
                services.GetRequiredService<IDiscordBotConfigurationService>();
            var discordBotConfiguration = discordConfigurationService.GetBotConfiguration();
            var client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += LogAsync;

            await client.LoginAsync(TokenType.Bot, discordBotConfiguration.Token);
            await client.StartAsync();
            await Task.Delay(Timeout.Infinite);
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<IDiscordCommonService, DiscordCommonService>()
                .AddSingleton<IDiscordBotConfigurationService, DiscordBotFileBasedConfigurationService>()
                .AddSingleton<IVoiceChannelManagementService, VoiceChannelManagementService>()
                .AddSingleton<IMessageFormattingService, MessageFormattingService>()
                .BuildServiceProvider();
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}