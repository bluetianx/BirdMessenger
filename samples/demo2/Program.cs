using BirdMessenger;
using demo2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddHttpClient<ITusClient, TusClient>();
        services.AddHostedService<Worker>();
        services.AddHostedService<Worker2>();
    })
    .RunConsoleAsync();
