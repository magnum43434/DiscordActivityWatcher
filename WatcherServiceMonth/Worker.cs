using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Library.Enums;
using Library.Models;
using Library.Utils;

namespace WatcherServiceMonth;


public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly HttpClient _httpClient;

    public Worker(ILogger<Worker> logger, HttpClientFactoryWrapper httpClientFactoryWrapper)
    {
        _logger = logger;
        // Create a new HttpClient instance using our wrapper.
        _httpClient = httpClientFactoryWrapper.CreateClient("default");
        _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("API_BASE_URL"));
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

            await CheckMonthTimeSpent();

            await Task.Delay(TimeSpan.FromHours(8),stoppingToken);
        }
    }

    private async Task CheckMonthTimeSpent()
    {
        foreach (var user in await GetUsers())
        {
            var result = await _httpClient.GetAsync($"api/Activities/guildIds?userId={user.Id}");
            var guildIds = await result.Content.ReadFromJsonAsync<IEnumerable<ulong>>() ?? Enumerable.Empty<ulong>();

            foreach (var guildId in guildIds)
            {
                var resultLastMonthId = await _httpClient.GetAsync("api/MonthTimeSpent/lastMonthId");
                var lastMonthId = await resultLastMonthId.Content.ReadFromJsonAsync<int>();
                await CreateMonthTimeSpent(user.Id, guildId, lastMonthId);
                var resultCurrentMonthId = await _httpClient.GetAsync("api/MonthTimeSpent/currentMonthId");
                var currentMonthId = await resultCurrentMonthId.Content.ReadFromJsonAsync<int>();
                await CreateMonthTimeSpent(user.Id, guildId, currentMonthId);
            }
        }
    }

    private async Task CreateMonthTimeSpent(Guid userId, ulong guildId, int monthId)
    {
        var timeCalculator = new TimeCalculator();
        var resultActivities = await _httpClient.GetAsync($"api/Activities/guild?userId={userId}&guildId={guildId}");
        var activities = await resultActivities.Content.ReadFromJsonAsync<IEnumerable<Activity>>();
        var monthTimeSpent = await GetMonthTimeSpent(userId, guildId, monthId);
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
                .FirstOrDefault(a => a.Action == ActivityAction.Joined && MonthIdGenerator.CompareMonthIdWithDateTime(monthId, a.Created));
            var switchedActivity = keyValuePair.Value
                .LastOrDefault(a => a.Action == ActivityAction.Switched && MonthIdGenerator.CompareMonthIdWithDateTime(monthId, a.Created));
            var leftActivity = keyValuePair.Value
                .FirstOrDefault(a => a.Action == ActivityAction.Left && MonthIdGenerator.CompareMonthIdWithDateTime(monthId, a.Created));
            
            TimeSpan timeSpan;
            if (joinActivity != null && leftActivity == null && switchedActivity != null) timeSpan = switchedActivity.Created - joinActivity.Created;
            else if (joinActivity != null && leftActivity != null) timeSpan = leftActivity.Created - joinActivity.Created;
            else continue;
            
            timeCalculator.Add((int)timeSpan.TotalHours, timeSpan.Minutes);
        }
        
        monthTimeSpent.TimeActiv = timeCalculator.ToString();
        monthTimeSpent.MinutesActiv = timeCalculator.TotalMinutes;

        if (monthTimeSpent.MinutesActiv > 0) 
            await UpdateMonthTimeSpent(monthTimeSpent);
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

    private async Task UpdateMonthTimeSpent(MonthTimeSpent monthTimeSpent)
    {
        try
        {
            var jsonObject = System.Text.Json.JsonSerializer.Serialize(monthTimeSpent);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            if (monthTimeSpent.Id != Guid.Empty)
            {
                var response = await _httpClient.PutAsync($"api/MonthTimeSpent/{monthTimeSpent.Id}", content);
                response.EnsureSuccessStatusCode();
            }
            else
            {
                var response = await _httpClient.PostAsync($"api/MonthTimeSpent", content);
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