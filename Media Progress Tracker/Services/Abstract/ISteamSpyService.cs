using Newtonsoft.Json.Linq;

namespace MediaProgressTracker.Services.Abstract
{
    public interface ISteamSpyService
    {
         Task<JArray> Return100AppsIn2Weeks();
    }
}