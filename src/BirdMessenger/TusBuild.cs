using BirdMessenger.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BirdMessenger
{
    public static class TusBuild
    {
        public static TusDefaultBuilder DefaultTusClientBuild(Uri tusHost)
        {
            return DefaultTusClientBuild((options) => { options.TusHost = tusHost; });
        }

        public static TusDefaultBuilder DefaultTusClientBuild(Action<TusClientOptions> configure)
        {
            IServiceCollection services = new ServiceCollection();
            var httpClientBuilder = services.AddTusClient(configure);
            return new TusDefaultBuilder(services, httpClientBuilder);
        }
    }
}