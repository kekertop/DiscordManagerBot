using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace DiscordChannelsBot.Services
{
    public class VoiceChannelManagementService
    {
        protected DiscordBotConfigurationService discordBotConfigurationService;
        protected Dictionary<ulong, CancellationTokenSource> voiceChannelDeletionCheckCancellationTokenSourcesDictionary;
        protected DiscordSocketClient discordClient;
        public VoiceChannelManagementService(IServiceProvider serviceProvider)
        {
            Console.WriteLine("VoiceChannelManagementService init start...");
            voiceChannelDeletionCheckCancellationTokenSourcesDictionary = new();
            discordBotConfigurationService = serviceProvider.GetRequiredService<DiscordBotConfigurationService>();
            discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
            discordClient.UserVoiceStateUpdated += UserVoiceStateUpdatedHandleAsync;
            Console.WriteLine("VoiceChannelManagementService init successfull...");
        }

        public async Task CreateVoiceChannelAsync(IGuild guild, string name)
        {
            await CreateVoiceChannelAsync(guild, name, null);
        }
        public async Task CreateVoiceChannelAsync(IGuild guild, string name, IEnumerable<IRole> roles)
        {
            ICategoryChannel categoryChannel = await discordBotConfigurationService.DiscordBotConfiguration.GetCategory(guild);
            IVoiceChannel voiceChannel;
            Action<VoiceChannelProperties> voiceChannelProperties = (channel) => { };
            if (categoryChannel != null)
            {
                voiceChannelProperties += (channel) => channel.CategoryId = categoryChannel.Id;
            }
            voiceChannel = await guild.CreateVoiceChannelAsync(name, voiceChannelProperties);
            if (roles != null && roles.Count() > 0)
            {
                await AllowOnlyRoles(guild, voiceChannel, roles);
            }
            RunDeletionCheck(voiceChannel.Id);

        }
        protected async Task AllowOnlyRoles(IGuild guild, IVoiceChannel voiceChannel, IEnumerable<IRole> roles)
        {
            OverwritePermissions rolePermissions = new OverwritePermissions().Modify(viewChannel: PermValue.Allow,
                connect: PermValue.Allow,
                speak: PermValue.Allow,
                useVoiceActivation: PermValue.Allow);

            OverwritePermissions denyPermissions = new OverwritePermissions().Modify(viewChannel: PermValue.Deny,
                connect: PermValue.Deny,
                speak: PermValue.Deny,
                useVoiceActivation: PermValue.Deny);

            await voiceChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, denyPermissions);

            foreach (var role in roles)
            {
                await voiceChannel.AddPermissionOverwriteAsync(role, rolePermissions);
            }
        }
        protected async Task UserVoiceStateUpdatedHandleAsync(SocketUser user, SocketVoiceState originalState, SocketVoiceState updatedState)
        {
            if (originalState.VoiceChannel != null)
            {
                SocketVoiceChannel voiceChannel = originalState.VoiceChannel;
                if (voiceChannel.Users.Count == 0)
                {
                    RunDeletionCheck(voiceChannel.Id);
                }
            }
            if (updatedState.VoiceChannel != null)
            {
                SocketVoiceChannel voiceChannel = updatedState.VoiceChannel;
                CancelDeletionCheckIfExists(voiceChannel.Id);
            }
        }
        protected Task RunDeletionCheck(ulong voiceChannelId)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Task deletionCheckTask = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                SocketVoiceChannel voiceChannel = (SocketVoiceChannel)discordClient.GetChannel(voiceChannelId);
                if (voiceChannel.Users.Count == 0)
                {
                    await voiceChannel.DeleteAsync();
                }
            });
            voiceChannelDeletionCheckCancellationTokenSourcesDictionary.Add(voiceChannelId, cancellationTokenSource);
            return deletionCheckTask;
        }

        protected void CancelDeletionCheckIfExists(ulong voiceChannelId)
        {
            if (voiceChannelDeletionCheckCancellationTokenSourcesDictionary.ContainsKey(voiceChannelId))
            {
                voiceChannelDeletionCheckCancellationTokenSourcesDictionary[voiceChannelId].Cancel();
                voiceChannelDeletionCheckCancellationTokenSourcesDictionary.Remove(voiceChannelId);
            }
        }
    }
}
