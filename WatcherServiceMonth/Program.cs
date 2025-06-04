using Library.Utils;
using WatcherServiceMonth;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<HttpClientFactoryWrapper>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();