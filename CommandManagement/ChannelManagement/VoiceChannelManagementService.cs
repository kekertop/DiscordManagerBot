using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordChannelsBot.CommandManagement.MessageFormatting;
using DiscordChannelsBot.Common;
using DiscordChannelsBot.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordChannelsBot.CommandManagement.ChannelManagement
{
    public class VoiceChannelManagementService : IVoiceChannelManagementService
    {
        private readonly IDiscordBotConfigurationService _discordBotConfigurationService;
        private readonly DiscordSocketClient _discordClient;
        private readonly IDiscordCommonService _discordCommonService;

        private readonly Dictionary<ulong, CancellationTokenSource>
            _voiceChannelDeletionCheckCancellationTokenSourcesDictionary;

        public VoiceChannelManagementService(IServiceProvider serviceProvider)
        {
            _voiceChannelDeletionCheckCancellationTokenSourcesDictionary =
                new Dictionary<ulong, CancellationTokenSource>();
            _discordBotConfigurationService = serviceProvider.GetRequiredService<IDiscordBotConfigurationService>();
            _discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
            _discordCommonService = serviceProvider.GetRequiredService<IDiscordCommonService>();
            _discordClient.UserVoiceStateUpdated += UserVoiceStateUpdatedHandleAsync;
        }

        public async Task CreateVoiceChannelAsync(IGuild guild, string name, GuildGroupsContext guildGroupsContext)
        {
            var guildConfiguration = _discordBotConfigurationService.GetGuildConfiguration(guild.Id);
            var categoryChannel =
                await _discordCommonService.GetCategoryAsync(guild, guildConfiguration.VoiceChannelCreationCategory);
            Action<VoiceChannelProperties> voiceChannelProperties = channel => { };
            if (categoryChannel != null) voiceChannelProperties += channel => channel.CategoryId = categoryChannel.Id;
            var voiceChannel = await guild.CreateVoiceChannelAsync(name, voiceChannelProperties);
            if (guildGroupsContext != null)
                if (guildGroupsContext.Roles != null && guildGroupsContext.Roles.Any() ||
                    guildGroupsContext.Users != null && guildGroupsContext.Users.Any())
                    await AllowOnlyRoles(guild, voiceChannel, guildGroupsContext);
            RunDeletionCheck(voiceChannel.Id);
        }

        private async Task AllowOnlyRoles(IGuild guild, IVoiceChannel voiceChannel,
            GuildGroupsContext guildGroupsContext)
        {
            var rolePermissions = new OverwritePermissions().Modify(viewChannel: PermValue.Allow,
                connect: PermValue.Allow,
                speak: PermValue.Allow);

            var denyPermissions = new OverwritePermissions().Modify(viewChannel: PermValue.Deny,
                connect: PermValue.Deny,
                speak: PermValue.Deny);

            await voiceChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, denyPermissions);

            await voiceChannel.AddPermissionOverwriteAsync(guildGroupsContext.CurrentUser, rolePermissions);
            if (guildGroupsContext.Roles != null)
                foreach (var role in guildGroupsContext.Roles)
                    await voiceChannel.AddPermissionOverwriteAsync(role, rolePermissions);
            if (guildGroupsContext.Users != null)
                foreach (var user in guildGroupsContext.Users)
                    await voiceChannel.AddPermissionOverwriteAsync(user, rolePermissions);
        }

        private Task UserVoiceStateUpdatedHandleAsync(SocketUser user, SocketVoiceState originalState,
            SocketVoiceState updatedState)
        {
            if (originalState.VoiceChannel != null)
            {
                var voiceChannel = originalState.VoiceChannel;
                if (voiceChannel.Users.Count == 0)
                    RunDeletionCheck(voiceChannel.Id);
            }

            if (updatedState.VoiceChannel != null)
            {
                var voiceChannel = updatedState.VoiceChannel;
                CancelDeletionCheckIfExists(voiceChannel.Id);
            }

            return Task.CompletedTask;
        }

        private Task RunDeletionCheck(ulong voiceChannelId)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var deletionCheckTask = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                if (cancellationToken.IsCancellationRequested) return;
                var voiceChannel = (SocketVoiceChannel) _discordClient.GetChannel(voiceChannelId);
                if (voiceChannel != null && voiceChannel.Users.Count == 0) await voiceChannel.DeleteAsync();
            });
            _voiceChannelDeletionCheckCancellationTokenSourcesDictionary.Add(voiceChannelId, cancellationTokenSource);
            return deletionCheckTask;
        }

        private void CancelDeletionCheckIfExists(ulong voiceChannelId)
        {
            if (_voiceChannelDeletionCheckCancellationTokenSourcesDictionary.ContainsKey(voiceChannelId))
            {
                _voiceChannelDeletionCheckCancellationTokenSourcesDictionary[voiceChannelId].Cancel();
                _voiceChannelDeletionCheckCancellationTokenSourcesDictionary.Remove(voiceChannelId);
            }
        }
    }
}