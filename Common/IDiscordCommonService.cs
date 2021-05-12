using System.Threading.Tasks;
using Discord;

namespace DiscordChannelsBot.Common
{
    public interface IDiscordCommonService
    {
        Task<ICategoryChannel> GetCategoryAsync(IGuild guild, string categoryName);
    }
}