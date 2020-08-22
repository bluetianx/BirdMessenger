using BirdMessenger.Abstractions;
using BirdMessenger.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using BirdMessenger.Infrastructure;

namespace BirdMessenger
{
    public  class TusBuild
    {
        private TusClientOption _tusClientOptions = new TusClientOption();
        
        private bool _tusBuilt;
        
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>(); 
        
        private List<Action<TusClientOption>> _configureClientActions= new List<Action<TusClientOption>>();

        

        public static TusBuild DefaultTusClientBuild(Uri tushost,string clientName="")
        {
            TusBuild tusBuild = new TusBuild();

            tusBuild.Configure((tusClientOption) =>
            {
                tusClientOption.TusHost = tushost;
                tusClientOption.ClientName = string.IsNullOrEmpty(clientName) ? "tusClient" : clientName;
                tusClientOption.Servces = new ServiceCollection();
                
                tusClientOption.Servces.AddHttpClient<ITusCore>( c =>
                {
                    c.DefaultRequestHeaders.Add("Tus-Resumable", "1.0.0");
                });
                tusClientOption.Servces.AddHttpClient<ITusExtension>( c =>
                {
                    c.DefaultRequestHeaders.Add("Tus-Resumable", "1.0.0");
                });

                tusClientOption.Servces.AddTransient<ITusCore, Tus>();
                tusClientOption.Servces.AddTransient<ITusExtension, Tus>();
                
            });
            
            return tusBuild;
            
        }

        public  TusBuild Configure(Action<TusClientOption> configAction)
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
            var serviceProvider = _tusClientOptions.Servces.BuildServiceProvider();

            var tusClient = serviceProvider.GetService<ITusClient>();
            if (tusClient == null)
            {
                var tusCore = serviceProvider.GetService<ITusCore>();
                var tusEx = serviceProvider.GetService<ITusExtension>();
                tusClient= new TusClient(tusCore,tusEx,
                    _tusClientOptions.TusHost,_tusClientOptions.GetUploadSize);
            }
            
            return tusClient;
        }

    }

    public class TusClientOption
    {
        /// <summary>
        /// tus server host
        /// </summary>
        public Uri TusHost { get; set; }

        public IServiceCollection Servces { get; set; }

        /// <summary>
        /// http factory clientName
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// first parameter is uploadedSize,second parameter is totalSize
        /// return size which will upload
        /// </summary>
        public Func<TusUploadContext, int> GetUploadSize = (context) => 1 * 1024 * 1024;
    }
}