using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BirdMessenger;
using System;

namespace demo2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient<ITusClient, TusClient>();
                    services.AddHostedService<Worker>();

                    services.AddHostedService<Worker2>();
                });
    }
}
