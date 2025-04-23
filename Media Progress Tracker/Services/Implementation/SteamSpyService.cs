using MediaProgressTracker.Services.Abstract;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace MediaProgressTracker.Services.Implementation
{
    class SteamSpyService : ISteamSpyService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://steamspy.com/api.php?request=";

        public SteamSpyService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public async Task<JArray> Return100AppsIn2Weeks()
        {
            var response = await _httpClient.GetAsync($"top100in2weeks");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            Console.Write(json);
            return (JArray)json["results"];
        }
    }
}
