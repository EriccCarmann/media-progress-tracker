using MediaProgressTracker.Models;

namespace MediaProgressTracker.Services.Abstract
{
    public interface ISteamSpyService
    {
        Task<IEnumerable<Game>> GetTop100In2WeeksAsync();
        Task ToJsonCS();
    }
}