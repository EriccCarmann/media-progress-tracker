using MediaProgressTracker.Models;
using Newtonsoft.Json.Linq;

namespace MediaProgressTracker.Services.Abstract
{
    public interface ISteamSpyService
    {
        Task GetAllGamesAsync();
        Task<IEnumerable<Game>> GetTop100In2WeeksAsync();
        public Game ReturnGame(JToken j);
        Task<Game> GetGameByAppIdAsync(int appId);
    }
}