using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Library.Enums;
using Library.Models;
using Library.Utils;

namespace WatcherService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly HttpClient _httpClient;

    public Worker(ILogger<Worker> logger, HttpClientFactoryWrapper httpClientFactoryWrapper)
    {
        _logger = logger;
        // Create a new HttpClient instance using our wrapper.
        _httpClient = httpClientFactoryWrapper.CreateClient("default");
        var apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL");
        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new InvalidOperationException("The environment variable 'API_BASE_URL' was not found or empty.");
        }
        _httpClient.BaseAddress = new Uri(apiBaseUrl);
        // Clear any existing Accept headers.
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        // Add the Accept header for "application/json".
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.UtcNow);

            await CheckTimeSpent();

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CheckTimeSpent()
    {
        foreach (var user in await GetUsers())
        {
            var result = await _httpClient.GetAsync($"api/Activities/guildIds?userId={user.Id}");
            var guildIds = await result.Content.ReadFromJsonAsync<IEnumerable<ulong>>() ?? Enumerable.Empty<ulong>();

            foreach (var guildId in guildIds)
            {
                await CreateTimeSpent(user.Id, guildId);
            }
        }
    }
    
    private async Task CreateTimeSpent(Guid userId, ulong guildId)
    {
        var timeCalculator = new TimeCalculator();
        var result = await _httpClient.GetAsync($"api/Activities/guild?userId={userId}&guildId={guildId}");
        var activities = await result.Content.ReadFromJsonAsync<IEnumerable<Activity>>();
        var timeSpent = await GetTimeSpent(userId, guildId);
        Dictionary<Guid, List<Activity>> transactions = new Dictionary<Guid, List<Activity>>();
        foreach (var activity in activities)
        {
            if (!transactions.TryGetValue(activity.TransactionId, out List<Activity>? value))
            {
                value = new List<Activity>();
                transactions.Add(activity.TransactionId, value); 
            }
            
            value.Add(activity);
        }

        foreach (var keyValuePair in transactions)
        {
            var joinActivity = keyValuePair.Value
                .FirstOrDefault(a => a.Action == ActivityAction.Joined);
            var switchedActivity = keyValuePair.Value
                .LastOrDefault(a => a.Action == ActivityAction.Switched);
            var leftActivity = keyValuePair.Value
                .FirstOrDefault(a => a.Action == ActivityAction.Left);
            
            TimeSpan timeSpan;
            if (joinActivity != null && leftActivity == null && switchedActivity != null) timeSpan = switchedActivity.Created - joinActivity.Created;
            else if (joinActivity != null && leftActivity != null) timeSpan = leftActivity.Created - joinActivity.Created;
            else continue;
            
            timeCalculator.Add((int)timeSpan.TotalHours, timeSpan.Minutes);
        }

        timeSpent.TimeActiv = timeCalculator.ToString();
        timeSpent.MinutesActiv = timeCalculator.TotalMinutes;
        timeSpent.LastActiv = activities.LastOrDefault().Created;

        await UpdateTimeSpent(timeSpent);
    }

    private async Task<TimeSpent> GetTimeSpent(Guid userId, ulong guildId)
    {
        var result = await _httpClient.GetAsync($"api/TimeSpent/guild?userId={userId}&guildId={guildId}");
        var timeSpent = await result.Content.ReadFromJsonAsync<TimeSpent>();
        if (!result.IsSuccessStatusCode || timeSpent == null)
        {
            timeSpent = new TimeSpent() {UserId = userId, GuildId = guildId};
        }

        return timeSpent;
    }
    
    private async Task<MonthTimeSpent> GetMonthTimeSpent(Guid userId, ulong guildId, int monthId)
    {
        var result = await _httpClient.GetAsync($"api/MonthTimeSpent/single?userId={userId}&guildId={guildId}&monthId={monthId}");
        var monthTimeSpent = await result.Content.ReadFromJsonAsync<MonthTimeSpent>();
        if (!result.IsSuccessStatusCode || monthTimeSpent == null)
        {
            monthTimeSpent = new MonthTimeSpent() {UserId = userId, GuildId = guildId, MonthId = monthId};
        }

        return monthTimeSpent;
    }

    private async Task UpdateTimeSpent(TimeSpent timeSpent)
    {
        try
        {
            var jsonObject = System.Text.Json.JsonSerializer.Serialize(timeSpent);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            if (timeSpent.Id != Guid.Empty)
            {
                var response = await _httpClient.PutAsync($"api/TimeSpent/{timeSpent.Id}", content);
                response.EnsureSuccessStatusCode();
            }
            else
            {
                var response = await _httpClient.PostAsync($"api/TimeSpent", content);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<IEnumerable<User>> GetUsers()
    {
        try
        {
            var result = await _httpClient.GetAsync("api/User");
            return await result.Content.ReadFromJsonAsync<IEnumerable<User>>() ?? Array.Empty<User>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}