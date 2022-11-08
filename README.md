<div style="text-align: left"><img src="docs/img/logo.png" height="120px">



# BirdMessenger

[![GitHub](https://img.shields.io/github/license/bluetianx/BirdMessenger)](LICENSE) [![NuGet](https://img.shields.io/nuget/v/BirdMessenger.svg?color=blue&style=popout-square)](https://www.nuget.org/packages/BirdMessenger) [![NuGet](https://img.shields.io/nuget/dt/BirdMessenger.svg)](https://www.nuget.org/packages/BirdMessenger)
>"Our aim is to solve the problem of unreliable file uploads once and for all. tus is a new open protocol for resumable uploads built on HTTP. It offers simple, cheap and reusable stacks for clients and servers. It supports any language, any platform and any network." - https://tus.io


BirdMessenger 中文名为：青鸟——相传为西王母的信使。
BirdMessnger 是一个基于.NET Standard 的 Tus协议的实现客户端。

## Features

### Protocol implementation

* Create
* HEAD
* PATCH
* OPTIONS
* DELETE

## Install

Package manager

``Install-Package BirdMessenger -Version 3.0.2``

.NET CLI

``dotnet add package BirdMessenger --version 3.0.2``

## Getting Started

```C#
public static Uri TusEndpoint = new Uri("http://localhost:5094/files");

        static async Task Main(string[] args)
        {
            await DemoUseTusClientByDependencyInjection();
            
            await DemoUseHttpClient();

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
            using var fileStream = new FileStream(fileInfo.FullName,FileMode.Open,FileAccess.Read);
            MetadataCollection metadata = new MetadataCollection();
            metadata["filename"] = fileInfo.Name;
            TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
            {
                Endpoint = TusEndpoint,
                Metadata = metadata,
                UploadLength = fileStream.Length
            };
            var resp = await tusClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);

            TusPatchRequestOption tusPatchRequestOption = new TusPatchRequestOption
            {
                FileLocation = resp.FileLocation,
                Stream = fileStream,
                OnProgressAsync = x =>
                {
                    var uploadedProgress = (int)Math.Floor(100 * (double)x.UploadedSize / x.TotalSize);
                    Console.WriteLine($"OnProgressAsync-TotalSize:{x.TotalSize}-UploadedSize:{x.UploadedSize}-uploadedProgress:{uploadedProgress}");
                    return Task.CompletedTask;
                },
                OnCompletedAsync = x =>
                {
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
        }
```

* You can see more examples in unit tests

## Document

[Wiki](https://github.com/bluetianx/BirdMessenger/wiki)

## Development

Development is done on the 'master' branch. 

## Who is using the library

* [China National Petroleum Corporation](https://www.cnpc.com.cn/cnpc/index.shtml)
* [BSS-ONE](https://www.bss-one.ro)

## Support and Sponsorship

<a href="https://www.jetbrains.com" target="_blank">
    <img src="./docs/img/jetbrains_logo.png" title="JetBrains" width="100" />
</a>
