using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger;
using BirdMessenger.Collections;
using BirdMessenger.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace demo
{
    class Program
    {
        public static Uri TusEndpoint = new Uri("http://localhost:5094/files");

        static async Task Main(string[] args)
        {
            await DemoUseTusClientByDependencyInjection();
            
            //await DemoUseHttpClient();

        }
        /// <summary>
        /// recommend using DependencyInjection 
        /// </summary>
        private static async Task DemoUseTusClientByDependencyInjection()
        {
            var services = new ServiceCollection();
            services.AddHttpClient<ITusClient, TusClient>(); // configure httpClient ,refer to https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            var serviceProvider = services.BuildServiceProvider();
            var tusClient = serviceProvider.GetService<ITusClient>();
            
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // file to be uploaded
            FileInfo fileInfo = new FileInfo(Path.Combine(location, @"TestFile/test.txt"));
            var fileStream = new FileStream(fileInfo.FullName,FileMode.Open,FileAccess.Read);
            MetadataCollection metadata = new MetadataCollection();
            metadata["filename"] = fileInfo.Name;
            TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
            {
                Endpoint = TusEndpoint,
                Metadata = metadata,
                UploadLength = fileStream.Length
            };
            var resp = await tusClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
            bool isInvokeOnProgressAsync = false;
            bool isInvokeOnCompletedAsync = false;
            long uploadedSize = 0;

            TusPatchRequestOption tusPatchRequestOption = new TusPatchRequestOption
            {
                FileLocation = resp.FileLocation,
                Stream = fileStream,
                OnProgressAsync = x =>
                {
                    isInvokeOnProgressAsync = true;
                    uploadedSize = x.UploadedSize;
                    var uploadedProgress = (int)Math.Floor(100 * (double)x.UploadedSize / x.TotalSize);
                    Console.WriteLine($"OnProgressAsync-TotalSize:{x.TotalSize}-UploadedSize:{x.UploadedSize}-uploadedProgress:{uploadedProgress}");
                    return Task.CompletedTask;
                },
                OnCompletedAsync = x =>
                {
                    isInvokeOnCompletedAsync = true;
                    var reqOption = x.TusRequestOption as TusPatchRequestOption;
                    Console.WriteLine($"File:{reqOption.FileLocation} Completed ");
                    return Task.CompletedTask;
                },
                OnFailedAsync = x =>
                {
                    Console.WriteLine($"error： {x.Exception.Message}");
                    if (x.OriginHttpRequestMessage is not null)
                    {
                        //log httpRequest
                    }

                    if (x.OriginResponseMessage is not null)
                    {
                        //log response
                    }
                    return Task.CompletedTask;
                }
            };

            var tusPatchResp = await tusClient.TusPatchAsync(tusPatchRequestOption, CancellationToken.None);
            // tusPatchResp.OriginResponseMessage
            // tusPatchResp.OriginHttpRequestMessage
        }

        /// <summary>
        /// using httpclient directly
        /// </summary>
        private static async Task DemoUseHttpClient()
        {
            using var httpClient = new HttpClient();
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // file to be uploaded
            FileInfo fileInfo = new FileInfo(Path.Combine(location, @"TestFile/test.txt"));
            
            var fileStream = new FileStream(fileInfo.FullName,FileMode.Open,FileAccess.Read);
            MetadataCollection metadata = new MetadataCollection();
            metadata["filename"] = fileInfo.Name;
            TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
            {
                Endpoint = TusEndpoint,
                Metadata = metadata,
                UploadLength = fileStream.Length
            };
            var resp = await httpClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
            bool isInvokeOnProgressAsync = false;
            bool isInvokeOnCompletedAsync = false;
            long uploadedSize = 0;

            TusPatchRequestOption tusPatchRequestOption = new TusPatchRequestOption
            {
                FileLocation = resp.FileLocation,
                Stream = fileStream,
                OnProgressAsync = x =>
                {
                    isInvokeOnProgressAsync = true;
                    uploadedSize = x.UploadedSize;
                    var uploadedProgress = (int)Math.Floor(100 * (double)x.UploadedSize / x.TotalSize);
                    Console.WriteLine($"OnProgressAsync-TotalSize:{x.TotalSize}-UploadedSize:{x.UploadedSize}-uploadedProgress:{uploadedProgress}");
                    return Task.CompletedTask;
                },
                OnCompletedAsync = x =>
                {
                    isInvokeOnCompletedAsync = true;
                    var reqOption = x.TusRequestOption as TusPatchRequestOption;
                    Console.WriteLine($"File:{reqOption.FileLocation} Completed ");
                    return Task.CompletedTask;
                },
                OnFailedAsync = x =>
                {
                    Console.WriteLine($"error： {x.Exception.Message}");
                    if (x.OriginHttpRequestMessage is not null)
                    {
                        //log httpRequest
                    }

                    if (x.OriginResponseMessage is not null)
                    {
                        //log response
                    }
                    return Task.CompletedTask;
                }
            };

            var tusPatchResp = await httpClient.TusPatchAsync(tusPatchRequestOption, CancellationToken.None);
            // tusPatchResp.OriginResponseMessage
            // tusPatchResp.OriginHttpRequestMessage
        }
        
        
    }
}