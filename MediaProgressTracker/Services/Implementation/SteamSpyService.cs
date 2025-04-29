using Firebase.Database;
using Firebase.Database.Query;
using MediaProgressTracker.Models;
using MediaProgressTracker.Services.Abstract;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.Json;

public class SteamSpyService : ISteamSpyService
{
    private readonly FirebaseClient _firebaseClient;
    private readonly HttpClient _httpClient;
    private readonly List<Game> gameListDb = new List<Game>();
    private const string BaseUrl = "https://steamspy.com/";

    public SteamSpyService(FirebaseClient firebaseClient)
    {
        _firebaseClient = firebaseClient;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };

        gameListDb.Clear();

        var gamesDb = _firebaseClient
                .Child("Games")
                .OnceAsync<Game>()
                .Result;

        foreach (var game in gamesDb)
        {
            gameListDb.Add(game.Object);
        }

        //var games = _firebaseClient
        //                .Child("Games")
        //                .AsObservable<Game>()
        //                .Subscribe(d =>
        //                {
        //                    if (d.Object != null)
        //                    {
        //                        var updatedGame = d.Object;
        //                    }
        //                });


        GetAllGamesAsync();


        //Task.Run(async () => await GetAllGamesAsync());

    }

    public async Task GetAllGamesAsync()
    {
        var games = new List<Game>();

        for (int i = 0; i <= 79; i++)
        {
            var response = await _httpClient.GetAsync($"api.php?request=all&page={i}");
            response.EnsureSuccessStatusCode();

            games = await GetGameData(response);
        }

        foreach (var game in games)
        {
            await _firebaseClient.Child("Games").PostAsync(game);
        }

    }

    public async Task<IEnumerable<Game>> GetTop100In2WeeksAsync()
    {
        //await ToJsonCS();

        //Console.WriteLine(File.ReadAllText("all_games.json"));
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

    public async Task ToJsonCS()
    {
        var response1 = await _httpClient.GetAsync("api.php?request=appdetails&appid=730");
        response1.EnsureSuccessStatusCode();

        string jsonContent = await response1.Content.ReadAsStringAsync();
        string json = JsonSerializer.Serialize(jsonContent);

        // Get the app's private storage path
        string folder = FileSystem.AppDataDirectory;
        // e.g. /data/user/0/com.yourcompany.yourapp/files
        string filePath = Path.Combine(folder, "all_games.json");

        File.WriteAllText(filePath, json);
    }

    public async Task<List<Game>> GetGameData(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(content);
        var games = new List<Game>(root.Count);

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

        foreach (var prop in root.Properties())
        {
            var j = (JObject)prop.Value;

            foreach (var game in gameListDb)
            {
                if (game.AppId.ToString() != prop.Name)
                {
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
            }
        }

        return games;
    }

    public async Task<Game> GetGameByAppIdAsync(int appId)
    {
        try
        {
            var matching = await _firebaseClient
                .Child("Games")
                .OrderBy("AppId")
                .EqualTo(appId)
                .OnceAsync<Game>();

            return matching.FirstOrDefault()?.Object;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving game with AppId {appId}: {ex.Message}");
            return null;
        }
    }
}