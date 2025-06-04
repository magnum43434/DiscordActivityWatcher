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

namespace DiscordBotCommand;

public class DiscordClient
{
    private DiscordSocketClient?  _client;
    private readonly HttpClient _httpClient;

    public DiscordClient(HttpClientFactoryWrapper httpClientFactoryWrapper)
    {
        // Create a new HttpClient instance using our wrapper.
        _httpClient = httpClientFactoryWrapper.CreateClient("default");
        _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("API_BASE_URL"));
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
        _client.Ready += ClientReady;
        _client.SlashCommandExecuted += SlashCommandHandler;
        
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN_COMMAND");
        
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
            foreach (var guild in _client.Guilds)
            {
                await _client.Rest.CreateGuildCommand(timeSpentCommand.Build(), guild.Id);
                await _client.Rest.CreateGuildCommand(topTenCommand.Build(), guild.Id);
            }
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
        // var result = await _httpClient.GetAsync($"api/TimeSpent/guild?userId={user.Id}&guildId={guildUser.Guild.Id}");
        var result = await _httpClient.GetAsync($"api/TimeSpent/guild?userId={user.Id}&guildId=107229471991427072");
        var timeSpent = await result.Content.ReadFromJsonAsync<TimeSpent>();

        var embedBuilder = new EmbedBuilder()
            .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle($"Time spent")
            .WithDescription(timeSpent.ForDisplay(guildUser.Nickname));
        
        await command.RespondAsync(embed: embedBuilder.Build());
    }
    
    private async Task HandleTopTenCommand(SocketSlashCommand command)
    {
        try
        {
            var guildId = command.GuildId ?? 0;
            var result  = await _httpClient.GetAsync($"api/TimeSpent/topten?guildId={guildId}");
            var topTenTimeSpent = await result.Content.ReadFromJsonAsync<IEnumerable<TimeSpent>>();
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < topTenTimeSpent.Count(); i++)
            {
                var timeSpent = topTenTimeSpent.ElementAt(i);
                var userResult = await _httpClient.GetAsync($"api/User/{timeSpent.UserId}");
                var user = await userResult.Content.ReadFromJsonAsync<User>();
                var guild = _client.GetGuild(timeSpent.GuildId);
                var guildUser = guild.GetUser(user.DiscordId);
                stringBuilder.AppendLine($"{i + 1}. {guildUser.Nickname} has spent {timeSpent.TimeActiv}.");
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Top 10 of users time spent")
                .WithDescription(stringBuilder.ToString());
        
            await command.RespondAsync(embed: embedBuilder.Build());
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
        return await result.Content.ReadFromJsonAsync<User>();
    }
}