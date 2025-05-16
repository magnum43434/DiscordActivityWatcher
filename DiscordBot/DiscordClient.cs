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
    private readonly string _apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL");
    private readonly ulong _guildId = Convert.ToUInt64(Environment.GetEnvironmentVariable("DISCORD_GUILD_ID"));

    public DiscordClient(HttpClientFactoryWrapper httpClientFactoryWrapper)
    {
        // Create a new HttpClient instance using our wrapper.
        _httpClient = httpClientFactoryWrapper.CreateClient("default");
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
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
            GatewayIntents = GatewayIntents.Guilds
        };
        _client = new DiscordSocketClient(config);
        
        _client.Log += Log;
        _client.Ready += ClientReady;
        _client.SlashCommandExecuted += SlashCommandHandler;
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

    private async Task ClientReady()
    {
        var timeSpentCommand = new Discord.SlashCommandBuilder()
            .WithName("time-spent")
            .WithDescription("Get time spent in voice channels for user.")
            .AddOption("user", ApplicationCommandOptionType.User, "The user you want to check their time spent.", isRequired: false);

        var topTenCommand = new Discord.SlashCommandBuilder()
            .WithName("top-ten")
            .WithDescription("Get the top ten of users time spent in voice channels.");

        try
        {
            await _client.Rest.CreateGuildCommand(timeSpentCommand.Build(), _guildId);
            await _client.Rest.CreateGuildCommand(topTenCommand.Build(), _guildId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch(command.Data.Name)
        {
            case "time-spent":
                await HandleTimeSpentCommand(command);
                break;
            case "top-ten":
                await HandleTopTenCommand(command);
                break;
        }
    }

    private async Task HandleTimeSpentCommand(SocketSlashCommand command)
    {
        var commandOption = command.Data.Options.FirstOrDefault();
        var guildUser = commandOption != null ? commandOption.Value as SocketGuildUser : command.User as SocketGuildUser;
        var user = await GetUser(guildUser);

        var embedBuilder = new EmbedBuilder()
            .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle($"Time spent")
            .WithDescription(user.ForDisplay())
            .WithCurrentTimestamp();
        
        await command.RespondAsync(embed: embedBuilder.Build());
    }
    
    private async Task HandleTopTenCommand(SocketSlashCommand command)
    {
        try
        {
            var result  = await _httpClient.GetAsync($"api/User/topten");
            var topTenUsers = await result.Content.ReadFromJsonAsync<IEnumerable<User>>();
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < topTenUsers.Count(); i++)
            {
                var user = topTenUsers.ElementAt(i);
                stringBuilder.AppendLine($"{i + 1}. {user.Nickname} has spent {user.TimeActiv}.");
            }
            
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Top 10 of users time spent")
                .WithDescription(stringBuilder.ToString())
                .WithCurrentTimestamp();
        
            await command.RespondAsync(embed: embedBuilder.Build());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task UserVoiceStateUpdatedEvent(SocketUser socketUser, SocketVoiceState oldState, SocketVoiceState newState)
    {
        User user = await GetUser(socketUser as SocketGuildUser);
        Activity activity;
        
        if (newState.VoiceChannel == null)
        {
            await CreateActivity(
                ActivityAction.Left, 
                oldState.VoiceChannel, 
                $"{user.Nickname} ({socketUser.Username}) has left the voice channel ({oldState.VoiceChannel?.Name})", 
                user);
            user.TransactionId = Guid.Empty;
        } 
        else if (oldState.VoiceChannel == null)
        {
            user.TransactionId = Guid.NewGuid();
            await CreateActivity(
                ActivityAction.Joined, 
                newState.VoiceChannel, 
                $"{user.Nickname} ({socketUser.Username}) has joined the voice channel ({newState.VoiceChannel?.Name})", 
                user);
        }
        else
        {
            await CreateActivity(
                ActivityAction.Switched, 
                newState.VoiceChannel, 
                $"{user.Nickname} ({socketUser.Username}) has switched the voice channel to {newState.VoiceChannel?.Name} from {oldState.VoiceChannel?.Name}", 
                user);
        }

        await UpdateUser(user);
    }

    private async Task CreateActivity(ActivityAction action, SocketVoiceChannel channel, string message, User user)
    {
        var activity = new Activity()
        {
            Created = DateTime.UtcNow,
            ChannelId = channel.Id,
            ChannelName = channel.Name,
            ChannelType = channel.ChannelType.ToString(),
            GuildId = channel.Guild.Id,
            GuildName = channel.Guild.Name,
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
        User user = await result.Content.ReadFromJsonAsync<User>();
        
        if (user?.DiscordId != 0) return user;
        user = new User()
        {
            DiscordId = socketUser.Id,
            Username = socketUser.Username,
            Nickname = socketUser.Nickname,
            AvatarUrl = socketUser.GetAvatarUrl(),
            TimeActiv = "",
            TransactionId = Guid.Empty
        };
        
        return user;
    }

    private async Task UpdateUser(User user)
    {
        try
        {
            var jsonObject = System.Text.Json.JsonSerializer.Serialize(user);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            if (user.Id != Guid.Empty)
            {
                var response = await _httpClient.PutAsync($"api/User/{user.Id}", content);
                response.EnsureSuccessStatusCode();
            }
            else
            {
                var response = await _httpClient.PostAsync($"api/User", content);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}