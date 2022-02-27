using Microsoft.EntityFrameworkCore;

namespace DiscordChannelsBot.Models;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.EnsureCreated();
        Database.Migrate();
    }

    public DbSet<DiscordGuildConfiguration> GuildConfigurations { get; set; }
}