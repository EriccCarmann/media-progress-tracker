using MediaProgressTracker.Models;

namespace MediaProgressTracker.Services.Abstract
{
    public interface ISteamSpyService
    {
        Task GetAllGamesAsync();
        Task<IEnumerable<Game>> GetTop100In2WeeksAsync();
        Task<List<Game>> GetGameData(HttpResponseMessage response);
        Task<Game> GetGameByAppIdAsync(int appId);
    }
}