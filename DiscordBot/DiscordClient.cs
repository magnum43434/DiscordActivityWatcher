using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Library.Enums;
using Library.Models;
using Library.Utils;
using Newtonsoft.Json;

namespace DiscordBot;

public class DiscordClient
{
    private DiscordSocketClient?  _client;
    private readonly HttpClient _httpClient;

    public DiscordClient(HttpClientFactoryWrapper httpClientFactoryWrapper)
    {
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

    public async Task InitializeAsync()
    {
        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged 
                             | GatewayIntents.GuildScheduledEvents 
                             | GatewayIntents.GuildVoiceStates 
                             | GatewayIntents.Guilds
        };
        _client = new DiscordSocketClient(config);
        
        _client.Log += Log;
        _client.UserVoiceStateUpdated += UserVoiceStateUpdatedEvent;
        
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        
        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
    
    private async Task UserVoiceStateUpdatedEvent(SocketUser socketUser, SocketVoiceState oldState, SocketVoiceState newState)
    {
        var guildUser = socketUser as SocketGuildUser;
        var user = await GetUser(guildUser);
        
        if (newState.VoiceChannel == null)
        {
            await CreateActivity(
                ActivityAction.Left, 
                oldState.VoiceChannel, 
                $"{guildUser.Nickname} ({socketUser.Username}) has left the voice channel ({oldState.VoiceChannel?.Name})", 
                user);
            user.TransactionId = Guid.Empty;
        } 
        else if (oldState.VoiceChannel == null)
        {
            user.TransactionId = Guid.NewGuid();
            await CreateActivity(
                ActivityAction.Joined, 
                newState.VoiceChannel, 
                $"{guildUser.Nickname} ({socketUser.Username}) has joined the voice channel ({newState.VoiceChannel?.Name})", 
                user);
        }
        else
        {
            await CreateActivity(
                ActivityAction.Switched, 
                newState.VoiceChannel, 
                $"{guildUser.Nickname} ({socketUser.Username}) has switched the voice channel to {newState.VoiceChannel?.Name} from {oldState.VoiceChannel?.Name}", 
                user);
        }

        await UpdateUser(user);
    }

    private async Task CreateActivity(ActivityAction action, SocketVoiceChannel channel, string message, User user)
    {
        await Log(new LogMessage(LogSeverity.Info, "UserVoiceStateUpdated", message));
        
        var activity = new Activity()
        {
            Created = DateTime.UtcNow,
            GuildId = channel.Guild.Id,
            Message = message,
            TransactionId = user.TransactionId,
            Action = action,
            UserId = user.Id
        };
        
        try
        {
            var jsonObject = System.Text.Json.JsonSerializer.Serialize(activity);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"api/Activities", content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private async Task<User> GetUser(SocketGuildUser socketUser)
    {
        var result  = await _httpClient.GetAsync($"api/User/discord/{socketUser.Id}");
        var user = await result.Content.ReadFromJsonAsync<User>();

        if (user?.DiscordId != 0) return user;
        user = new User()
        {
            DiscordId = socketUser.Id,
            Username = socketUser.Username,
            AvatarUrl = socketUser.GetAvatarUrl(),
            TransactionId = Guid.Empty
        };

        user = await UpdateUser(user);

        return user;
    }

    private async Task<User> UpdateUser(User user)
    {
        try
        {
            var jsonObject = System.Text.Json.JsonSerializer.Serialize(user);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            if (user.Id != Guid.Empty)
            {
                var response = await _httpClient.PutAsync($"api/User/{user.Id}", content);
                response.EnsureSuccessStatusCode();
                return user;
            }
            else
            {
                var response = await _httpClient.PostAsync($"api/User", content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<User>();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}