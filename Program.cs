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
            await using var serviceProvider = ConfigureServices();
            await InitializeServices(serviceProvider);

            var discordConfigurationService =
                serviceProvider.GetRequiredService<IDiscordBotConfigurationService>();
            var client = serviceProvider.GetRequiredService<DiscordSocketClient>();

            var discordBotConfiguration = discordConfigurationService.GetBotConfiguration();

            client.Log += LogAsync;

            await client.LoginAsync(TokenType.Bot, discordBotConfiguration.Token);
            await client.StartAsync();
            await Task.Delay(Timeout.Infinite);
        }

        private static async Task InitializeServices(IServiceProvider serviceProvider)
        {
            await serviceProvider.GetRequiredService<ICommandHandlingService>().InitializeAsync();
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<IDiscordCommonService, DiscordCommonService>()
                .AddSingleton<IDiscordBotConfigurationService, DiscordBotFileBasedConfigurationService>()
                .AddSingleton<ICommandHandlingService, CommandHandlingService>()
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