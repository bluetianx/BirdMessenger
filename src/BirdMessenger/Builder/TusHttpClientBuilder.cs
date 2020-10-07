using Microsoft.Extensions.DependencyInjection;
using System;

namespace BirdMessenger.Builder
{
    public class TusHttpClientBuilder : ITusHttpClientBuilder<TusHttpClientBuilder>
    {
        internal readonly TusClientOptions _tusClientOptions;
        internal readonly IHttpClientBuilder _coreHttpClientBuilder;
        internal readonly IHttpClientBuilder _extensionHttpClientBuilder;

        internal TusHttpClientBuilder(TusClientOptions tusClientOptions, IHttpClientBuilder coreHttpClientBuilder, IHttpClientBuilder extensionHttpClientBuilder)
        {
            _tusClientOptions = tusClientOptions;
            _coreHttpClientBuilder = coreHttpClientBuilder;
            _extensionHttpClientBuilder = extensionHttpClientBuilder;
        }

        public TusHttpClientBuilder Configure(Action<TusClientOptions, IHttpClientBuilder> builder)
        {
            ConfigureCore(builder);
            ConfigureExtension(builder);
            return this;
        }
        public TusHttpClientBuilder ConfigureCore(Action<TusClientOptions, IHttpClientBuilder> builder)
        {
            builder(_tusClientOptions, _coreHttpClientBuilder);
            return this;
        }
        public TusHttpClientBuilder ConfigureExtension(Action<TusClientOptions, IHttpClientBuilder> builder)
        {
            builder(_tusClientOptions, _extensionHttpClientBuilder);
            return this;
        }
    }
}
