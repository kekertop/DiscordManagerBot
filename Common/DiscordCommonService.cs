using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace DiscordChannelsBot.Common
{
    public class DiscordCommonService : IDiscordCommonService
    {
        public async Task<ICategoryChannel> GetCategoryAsync(IGuild guild, string categoryName)
        {
            var categoryChannels = await guild.GetCategoriesAsync();
            return categoryChannels.FirstOrDefault(category => category.Name.Equals(categoryName));
        }
    }
}