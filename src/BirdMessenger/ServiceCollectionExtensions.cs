using BirdMessenger.Abstractions;
using BirdMessenger.Builder;
using BirdMessenger.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BirdMessenger
{
    public static class ServiceCollectionExtensions
    {
        public static TusHttpClientBuilder AddTusClient(this IServiceCollection services, Uri tusHost)
        {
            return services.AddTusClient((opts) => { opts.TusHost = tusHost; });
        }

        public static TusHttpClientBuilder AddTusClient(this IServiceCollection services, Action<TusClientOptions> configure)
        {
            var options = new TusClientOptions();
            configure(options);

            var coreHttpClientBuilder = services.AddHttpClient<ITusCore, Tus>(c =>
            {
                c.DefaultRequestHeaders.Add("Tus-Resumable", "1.0.0");
            });
            var extensionHttpClientBuilder = services.AddHttpClient<ITusExtension, Tus>(c =>
            {
                c.DefaultRequestHeaders.Add("Tus-Resumable", "1.0.0");
            });
            services.AddSingleton<ITusClientOptions>(options);
            services.AddTransient<ITusClient>((services) =>
            {
                var tusCore = services.GetService<ITusCore>();
                var tusExtension = services.GetService<ITusExtension>();
                var opts = options;
                return new TusClient(tusCore, tusExtension, opts);
            });
            return new TusHttpClientBuilder(options, coreHttpClientBuilder, extensionHttpClientBuilder);
        }
    }
}
