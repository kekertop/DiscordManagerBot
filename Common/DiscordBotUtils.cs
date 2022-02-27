using Discord;

namespace DiscordChannelsBot.Common;

public static class DiscordBotUtils
{
    public static async Task<ICategoryChannel> GetCategoryAsync(IGuild guild, string categoryName)
    {
        var categoryChannels = await guild.GetCategoriesAsync();
        return categoryChannels.FirstOrDefault(category => category.Name.Equals(categoryName));
    }
}