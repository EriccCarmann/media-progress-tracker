using Firebase.Database;
using Firebase.Database.Query;
using MediaProgressTracker.Models;
using MediaProgressTracker.Services.Abstract;
using Newtonsoft.Json.Linq;
using System.Globalization;

public class SteamSpyService : ISteamSpyService
{
    private readonly FirebaseClient _firebaseClient;
    private readonly HttpClient _httpClient;
    private readonly List<Game> gameListDb = new List<Game>();
    private readonly IExceptionHandler _exceptionHandler;
    private const string BaseUrl = "https://steamspy.com/";

    public SteamSpyService(FirebaseClient firebaseClient, IExceptionHandler exceptionHandler)
    {
        _firebaseClient = firebaseClient;
        _exceptionHandler = exceptionHandler;

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

        Task.Run(async () => await GetAllGamesAsync());
    }

    public async Task GetAllGamesAsync()
    {
        for (int i = 0; i <= 79; i++)
        {
            var response = await _exceptionHandler.HandleAsync(async () =>
            {
                var resp = await _httpClient.GetAsync($"api.php?request=all&page={i}");
                resp.EnsureSuccessStatusCode();
                return resp;
            });

            var content = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(content);

            foreach (var prop in root.Properties())
            {
                var j = (JObject)prop.Value;

                var gameExists = await GameExistsAsync(int.Parse(prop.Name));

                if (!gameExists)
                {
                    await _firebaseClient.Child("Games").PostAsync(ReturnGame(j));
                }
            }
        }
    }

    public async Task<IEnumerable<Game>> GetTop100In2WeeksAsync()
    {
        var response = await _exceptionHandler.HandleAsync(async () =>
        {
            var resp = await _httpClient.GetAsync("api.php?request=top100in2weeks");
            resp.EnsureSuccessStatusCode();
            return resp;
        });

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

            games.Add(ReturnGame(j));
        }

        return games;
    }

    public Game ReturnGame(JToken j)
    {
        return new Game
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
        };
    }

    private decimal ParseDecimal(JToken token)
    {
        if (token == null) return 0m;
        var s = token.Type == JTokenType.String
                ? (string)token
                : token.ToString();
        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
             ? d
             : 0m;
    }

    private int ParseInt(JToken token)
    {
        if (token == null) return 0;
        return int.TryParse(token.ToString(), out var i)
        ? i
        : 0;
    }


    public async Task<bool> GameExistsAsync(int appId)
    {
        var snapshot = await _firebaseClient
            .Child("Games")
            .OrderBy("AppId")
            .EqualTo(appId)
            .OnceAsync<Game>();

        return snapshot.Any();
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

    public async Task<Game> GetGameByNameAsync(string name)
    {
        try
        {
            var matching = await _firebaseClient
                .Child("Games")
                .OrderBy("Name")
                .EqualTo(name)
                .OnceAsync<Game>();

            return matching.FirstOrDefault()?.Object;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving game with AppId {name}: {ex.Message}");
            return null;
        }
    }
}