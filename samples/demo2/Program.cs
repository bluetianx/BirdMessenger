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
                    var hostUri = new Uri(@"http://localhost:5000/files");
                    services.AddTusClient<Worker>(hostUri);
                    services.AddHostedService<Worker>();

                    var hostUri2 = new Uri(@"http://localhost:5001/files");
                    services.AddTusClient<Worker2>(hostUri2);
                    services.AddHostedService<Worker2>();
                });
    }
}
