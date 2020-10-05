using BirdMessenger.Abstractions;
using BirdMessenger.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using BirdMessenger.Infrastructure;

namespace BirdMessenger
{
    public sealed class DefaultTusBuild : TusBuild
    {
        private DefaultTusBuild()
        {
            _tusClientOptions = new DefaultTusClientOption();
        }

        public DefaultTusClientOption Options => _tusClientOptions as DefaultTusClientOption;
        public static DefaultTusBuild DefaultTusClientBuild(Uri tushost, string clientName = "")
        {
            DefaultTusBuild tusBuild = new DefaultTusBuild();

            tusBuild.Configure((tusClientOption) =>
            {
                tusClientOption.TusRemoteHost = tushost;
                tusClientOption.Services = new ServiceCollection();

                (tusClientOption as DefaultTusClientOption).CoreHttpClientBuilder = tusClientOption.Services.AddHttpClient<ITusCore, Tus>(c =>
                {
                    c.DefaultRequestHeaders.Add("Tus-Resumable", "1.0.0");
                });
                (tusClientOption as DefaultTusClientOption).ExtensionHttpClientBuilder = tusClientOption.Services.AddHttpClient<ITusExtension, Tus>(c =>
                {
                    c.DefaultRequestHeaders.Add("Tus-Resumable", "1.0.0");
                });
            });
            return tusBuild;
        }
    }

    public class TusBuild
    {
        protected TusClientOption _tusClientOptions;

        public TusBuild()
        {
            _tusClientOptions = new TusClientOption();
        }

        private bool _tusBuilt;
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
        private List<Action<TusClientOption>> _configureClientActions = new List<Action<TusClientOption>>();

        public TusBuild Configure(Action<TusClientOption> configAction)
        {
            _configureClientActions.Add(configAction ?? throw new ArgumentNullException(nameof(configAction)));
            return this;
        }

        public ITusClient Build()
        {
            if (_tusBuilt)
            {
                throw new InvalidOperationException("Build can only be called once.");
            }
            _tusBuilt = true;
            foreach (var configureClientAction in _configureClientActions)
            {
                configureClientAction(_tusClientOptions);
            }
            var serviceProvider = _tusClientOptions.Services.BuildServiceProvider();

            var tusClient = serviceProvider.GetService<ITusClient>();
            if (tusClient == null)
            {
                var tusCore = serviceProvider.GetService<ITusCore>();
                var tusEx = serviceProvider.GetService<ITusExtension>();
                tusClient = new TusClient(tusCore, tusEx, _tusClientOptions.TusRemoteHost, _tusClientOptions.GetUploadChunkSize);
            }

            return tusClient;
        }
    }

    public class TusClientOption
    {
        /// <summary>
        /// tus server host
        /// </summary>
        public Uri TusRemoteHost { get; set; }

        public IServiceCollection Services { get; set; }

        /// <summary>
        /// first parameter is uploadedSize,second parameter is totalSize
        /// return size which will upload
        /// </summary>
        public Func<TusUploadContext, int> GetUploadChunkSize = (context) => 1 * 1024 * 1024;
    }

    public class DefaultTusClientOption : TusClientOption
    {
        public IHttpClientBuilder CoreHttpClientBuilder { get; internal set; }
        public IHttpClientBuilder ExtensionHttpClientBuilder { get; internal set; }
    }
}