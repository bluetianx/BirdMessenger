using Microsoft.Extensions.DependencyInjection;
using System;

namespace BirdMessenger.Builder
{
    public class TusHttpClientConfiguration : ITusHttpClientConfiguration<TusHttpClientConfiguration>
    {
        private readonly TusClientOptions _tusClientOptions;
        private readonly IHttpClientBuilder _coreHttpClientBuilder;
        private readonly IHttpClientBuilder _extensionHttpClientBuilder;

        internal TusHttpClientConfiguration(TusClientOptions tusClientOptions, IHttpClientBuilder coreHttpClientBuilder, IHttpClientBuilder extensionHttpClientBuilder)
        {
            _tusClientOptions = tusClientOptions;
            _coreHttpClientBuilder = coreHttpClientBuilder;
            _extensionHttpClientBuilder = extensionHttpClientBuilder;
        }

        public TusHttpClientConfiguration Configure(Action<TusClientOptions, IHttpClientBuilder> builder)
        {
            ConfigureCore(builder);
            ConfigureExtension(builder);
            return this;
        }
        public TusHttpClientConfiguration ConfigureCore(Action<TusClientOptions, IHttpClientBuilder> builder)
        {
            builder(_tusClientOptions, _coreHttpClientBuilder);
            return this;
        }
        public TusHttpClientConfiguration ConfigureExtension(Action<TusClientOptions, IHttpClientBuilder> builder)
        {
            builder(_tusClientOptions, _extensionHttpClientBuilder);
            return this;
        }
    }
}
