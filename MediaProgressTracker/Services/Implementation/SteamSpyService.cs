using MediaProgressTracker.Models;
using MediaProgressTracker.Services.Abstract;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        await ToJson();

        Console.WriteLine(File.ReadAllText("all_games.json"));
        //var content1 = await response1.Content.ReadAsStringAsync();

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

    public async Task ToJson()
    {
        var response1 = await _httpClient.GetAsync("api.php?request=appdetails&appid=730");
        response1.EnsureSuccessStatusCode();

        string jsonContent = await response1.Content.ReadAsStringAsync();
        string json = JsonSerializer.Serialize(jsonContent);

        // Get the app’s private storage path
        string folder = FileSystem.AppDataDirectory;
        // e.g. /data/user/0/com.yourcompany.yourapp/files
        string filePath = Path.Combine(folder, "all_games.json");

        File.WriteAllText(filePath, json);
    }
}