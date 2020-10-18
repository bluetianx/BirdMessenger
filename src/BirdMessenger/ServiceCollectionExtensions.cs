using BirdMessenger.Abstractions;
using BirdMessenger.Builder;
using BirdMessenger.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace BirdMessenger
{
    public static class ServiceCollectionExtensions
    {
        private static void DefaultHttpClientConfigure(HttpClient c)
        {
            c.DefaultRequestHeaders.Add("Tus-Resumable", "1.0.0");
        }

        public static TusHttpClientConfiguration AddTusClient(this IServiceCollection services, Uri tusHost)
        {
            return services.AddTusClient((opts) => { opts.TusHost = tusHost; });
        }

        public static TusHttpClientConfiguration AddTusClient(this IServiceCollection services, Action<TusClientOptions> configure)
        {
            var options = new TusClientOptions();
            configure(options);

            var coreHttpClientBuilder = services.AddHttpClient<ITusCore, Tus>(httpClient =>
            {
                DefaultHttpClientConfigure(httpClient);
            });
            var extensionHttpClientBuilder = services.AddHttpClient<ITusExtension, Tus>(httpClient =>
            {
                DefaultHttpClientConfigure(httpClient);
            });
            services.AddTransient<ITusClient>((services) =>
            {
                var tusCore = services.GetService<ITusCore>();
                var tusExtension = services.GetService<ITusExtension>();
                var opts = options;
                return new TusClient(tusCore, tusExtension, opts);
            });
            return new TusHttpClientConfiguration(options, coreHttpClientBuilder, extensionHttpClientBuilder);
        }

        public static TusHttpClientConfiguration AddTusClient<TService>(this IServiceCollection services, Uri tusHost)
        {
            return services.AddTusClient<TService>((opts) => { opts.TusHost = tusHost; });
        }

        public static TusHttpClientConfiguration AddTusClient<TService>(this IServiceCollection services, Action<TusClientOptions> configure)
        {
            var options = new TusClientOptions();
            configure(options);

            var coreHttpClientBuilder = services.AddHttpClient<ITusCore<TService>, Tus<TService>>(httpClient =>
            {
                DefaultHttpClientConfigure(httpClient);
            });
            var extensionHttpClientBuilder = services.AddHttpClient<ITusExtension<TService>, Tus<TService>>(httpClient =>
            {
                DefaultHttpClientConfigure(httpClient);
            });
            services.AddTransient<ITusClient<TService>>((services) =>
            {
                var tusCore = services.GetService<ITusCore<TService>>();
                var tusExtension = services.GetService<ITusExtension<TService>>();
                var opts = options;
                return new TusClient<TService>(tusCore, tusExtension, opts);
            });
            return new TusHttpClientConfiguration(options, coreHttpClientBuilder, extensionHttpClientBuilder);
        }
    }
}
