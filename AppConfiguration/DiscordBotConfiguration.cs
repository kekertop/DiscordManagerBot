using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;

namespace DiscordChannelsBot.AppConfiguration
{
    public class DiscordBotConfiguration
    {
        [JsonProperty("discordToken")]
        public string DiscordToken { get; set; }
        [JsonProperty("discordPrefix")]
        public string DiscordPrefix { get; set; }
        [JsonProperty("voiceChannelCreationCategory")]
        protected Dictionary<ulong, String> voiceChannelCreationCategory { get; set; }

        public async Task<ICategoryChannel> GetCategory(IGuild guild)
        {
            if(voiceChannelCreationCategory.ContainsKey(guild.Id))
            {
                string categoryName = voiceChannelCreationCategory[guild.Id];
                IReadOnlyCollection<ICategoryChannel> categoryChannels = await guild.GetCategoriesAsync();
                return categoryChannels.Where(category => category.Name.Equals(categoryName)).FirstOrDefault();
            }
            return null;
        }

        public void SetCategory(ulong guildId, string name)
        {
            if (voiceChannelCreationCategory.ContainsKey(guildId))
            {
                voiceChannelCreationCategory[guildId] = name;
            }
            else
            {
                voiceChannelCreationCategory.Add(guildId, name);
            }
        } 
    }
}
