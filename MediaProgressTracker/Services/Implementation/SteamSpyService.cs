using MediaProgressTracker.Models;
using MediaProgressTracker.Services.Abstract;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http;

public class SteamSpyService : ISteamSpyService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://steamspy.com/";

    public SteamSpyService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }

    public async Task<IEnumerable<Game>> GetTop100In2WeeksAsync()
    {
        var response = await _httpClient.GetAsync("api.php?request=top100in2weeks");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(content);
        var games = new List<Game>(root.Count);

        foreach (var prop in root.Properties())
        {
            var j = (JObject)prop.Value;

            decimal ParseDecimal(JToken token)
            {
                if (token == null) return 0m;
                var s = token.Type == JTokenType.String
                        ? (string)token
                        : token.ToString();
                return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                     ? d
                     : 0m;
            }

            int ParseInt(JToken token)
            {
                if (token == null) return 0;
                return int.TryParse(token.ToString(), out var i)
                     ? i
                     : 0;
            }

            games.Add(new Game
            {
                AppId = ParseInt(j["appid"]),
                Name = (string)j["name"] ?? "",
                Developer = (string)j["developer"] ?? "",
                Publisher = (string)j["publisher"] ?? "",
                ScoreRank = j["score_rank"]?.ToString() ?? "",
                PositiveReviews = ParseInt(j["positive"]),
                NegativeReviews = ParseInt(j["negative"]),
                UserScore = ParseInt(j["userscore"]),
                Owners = (string)j["owners"] ?? "",
                AverageForever = ParseDecimal(j["average_forever"]),
                Average2Weeks = ParseDecimal(j["average_2weeks"]),
                MedianForever = ParseDecimal(j["median_forever"]),
                Median2Weeks = ParseDecimal(j["median_2weeks"]),
                Price = ParseDecimal(j["price"]),
                InitialPrice = ParseDecimal(j["initialprice"]),
                Discount = ParseDecimal(j["discount"]),
                CCU = ParseInt(j["ccu"])
            });
        }

        return games;
    }
}