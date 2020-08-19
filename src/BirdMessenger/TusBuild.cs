using BirdMessenger.Abstractions;
using BirdMessenger.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using BirdMessenger.Infrastructure;
using System.Collections.Generic;

namespace BirdMessenger
{
    public  class TusBuild
    {
        private TusClientOption _tusClientOptions;

        public TusBuild(TusClientOption tusClientOption)
        {
            _tusClientOptions = tusClientOption;
        }

        public static TusBuild DefaultTusClientBuild(Uri tushost,string clientName="")
        {
            TusClientOption tusClientOption = new TusClientOption
            {
                TusHost = tushost,
                ClientName=string.IsNullOrEmpty(clientName) ?"tusClient" : clientName
            };

            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient(tusClientOption.ClientName, c =>
            {
                c.DefaultRequestHeaders.Add("Tus-Resumable", "1.0.0");
            });

            services.AddTransient<ITusCore, Tus>();
            services.AddTransient<ITusExtension, Tus>();

            tusClientOption.Servces = services;

            TusBuild tusBuild = new TusBuild(tusClientOption);
            return tusBuild;
            
        }

        public static TusBuild DefaultTusClientBuild(Uri tushost, Dictionary<string, string> header, string clientName = "")
        {
            TusClientOption tusClientOption = new TusClientOption
            {
                TusHost = tushost,
                ClientName = string.IsNullOrEmpty(clientName) ? "tusClient" : clientName
            };

            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient(tusClientOption.ClientName, c =>
            {
                c.DefaultRequestHeaders.Add("Tus-Resumable", "1.0.0");
                if (header != null)
                {
                    foreach (var pair in header)
                    {
                        c.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                    }
                }
            });

            services.AddTransient<ITusCore, Tus>();
            services.AddTransient<ITusExtension, Tus>();

            tusClientOption.Servces = services;

            TusBuild tusBuild = new TusBuild(tusClientOption);
            return tusBuild;

        }

        public  TusBuild Configure(Action<TusClientOption> configAction)
        {
            configAction(this._tusClientOptions);
            return this;
        }

        public TusClient Build()
        {
            var serviceProvider = _tusClientOptions.Servces.BuildServiceProvider();
            var tusClient = new TusClient(serviceProvider,_tusClientOptions.ClientName,
                _tusClientOptions.TusHost,_tusClientOptions.GetUploadSize);

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