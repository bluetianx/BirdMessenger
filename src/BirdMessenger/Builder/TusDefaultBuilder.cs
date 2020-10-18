using Microsoft.Extensions.DependencyInjection;
using System;

namespace BirdMessenger.Builder
{
    public class TusDefaultBuilder : ITusHttpClientConfiguration<TusDefaultBuilder>
    {
        private readonly IServiceCollection _services;
        private readonly TusHttpClientConfiguration _tusHttpClientBuilder;

        internal TusDefaultBuilder(IServiceCollection services, TusHttpClientConfiguration tusHttpClientBuilder)
        {
            _services = services;
            _tusHttpClientBuilder = tusHttpClientBuilder;
        }

        public TusDefaultBuilder Configure(Action<TusClientOptions, IHttpClientBuilder> builder)
        {
            _tusHttpClientBuilder.Configure(builder);
            return this;
        }
        public TusDefaultBuilder ConfigureCore(Action<TusClientOptions, IHttpClientBuilder> builder)
        {
            _tusHttpClientBuilder.ConfigureCore(builder);
            return this;
        }
        public TusDefaultBuilder ConfigureExtension(Action<TusClientOptions, IHttpClientBuilder> builder)
        {
            _tusHttpClientBuilder.ConfigureExtension(builder);
            return this;
        }

        public ITusClient Build()
        {
            var provider = _services.BuildServiceProvider();
            return provider.GetService<ITusClient>();
        }
    }
}
