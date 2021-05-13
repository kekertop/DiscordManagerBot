using System.Threading.Tasks;

namespace DiscordChannelsBot.CommandManagement.CommandHandling
{
    public interface ICommandHandlingService
    {
        Task InitializeAsync();
    }
}