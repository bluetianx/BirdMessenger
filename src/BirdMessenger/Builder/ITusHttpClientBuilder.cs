using Microsoft.Extensions.DependencyInjection;
using System;

namespace BirdMessenger.Builder
{
    public interface ITusHttpClientBuilder<TService>
    {
        public TService Configure(Action<TusClientOptions, IHttpClientBuilder> builder);
        public TService ConfigureCore(Action<TusClientOptions, IHttpClientBuilder> builder);
        public TService ConfigureExtension(Action<TusClientOptions, IHttpClientBuilder> builder);
    }
}
