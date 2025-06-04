using DiscordBotCommand;
using Library.Utils;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register IHttpClientFactory along with default HttpClient.
services.AddHttpClient();

// Register the custom HttpClientFactory wrapper as a singleton.
services.AddSingleton<HttpClientFactoryWrapper>();

// Register our custom service as a transient dependency.
services.AddTransient<DiscordClient>();

// Build the service provider.
var serviceProvider = services.BuildServiceProvider();

// Retrieve an instance of our custom service.
var discordClient = serviceProvider.GetRequiredService<DiscordClient>();

await discordClient.InitializeAsync();