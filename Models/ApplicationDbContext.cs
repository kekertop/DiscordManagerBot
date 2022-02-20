using Microsoft.EntityFrameworkCore;

namespace DiscordChannelsBot.Models;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
        Database.Migrate();
    }

    public DbSet<DiscordGuildConfiguration> GuildConfigurations { get; set; }
}