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
    private readonly string _apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL");

    public Worker(ILogger<Worker> logger, HttpClientFactoryWrapper httpClientFactoryWrapper)
    {
        _logger = logger;
        // Create a new HttpClient instance using our wrapper.
        _httpClient = httpClientFactoryWrapper.CreateClient("default");
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
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

            foreach (var user in await GetUsers())
            {
                var timeCalculator = new TimeCalculator();
                Dictionary<Guid, List<Activity>> transactions = new Dictionary<Guid, List<Activity>>();
                var activities = await GetActivities(user.Id);
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
                    var leftActivity = keyValuePair.Value
                        .FirstOrDefault(a => a.Action == ActivityAction.Left);
                    if (joinActivity == null || leftActivity == null) continue;
                    
                    var timeSpent = leftActivity.Created - joinActivity.Created;
                    timeCalculator.Add((int)timeSpent.TotalHours, timeSpent.Minutes);
                }

                user.TimeActiv = timeCalculator.ToString();
                user.MinutesActiv = timeCalculator.TotalMinutes;
                user.LastActiv = activities.LastOrDefault().Created;
                await UpdateUser(user);
            }

            await Task.Delay(5000, stoppingToken);
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

    private async Task UpdateUser(User user)
    {
        var jsonObject = System.Text.Json.JsonSerializer.Serialize(user);
        var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync($"api/User/{user.Id}", content);
        response.EnsureSuccessStatusCode();
    }

    private async Task<IEnumerable<Activity>> GetActivities(Guid userId)
    {
        try
        {
            var result = await _httpClient.GetAsync($"api/activities/user/{userId}");
            return await result.Content.ReadFromJsonAsync<IEnumerable<Activity>>() ?? Array.Empty<Activity>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}