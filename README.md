# BirdMessenger
[![NuGet](https://img.shields.io/nuget/v/BirdMessenger.svg?color=blue&style=popout-square)](https://www.nuget.org/packages/BirdMessenger)[![NuGet](https://img.shields.io/nuget/dt/BirdMessenger.svg)](https://www.nuget.org/packages/BirdMessenger)
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

``Install-Package BirdMessenger -Version 1.0.1``

.NET CLI

``dotnet add package BirdMessenger --version 1.0.1``

## Getting Started

```C#

            FileInfo fileInfo = new FileInfo("test");           
            var hostUri = new Uri(@"http://localhost:5000/files");
            var tusClient=TusBuild.DefaultTusClientBuild(hostUri)
                .Build();
            tusClient.Uploading += printUploadProcess;
            tusClient.UploadFinish += UploadFinish;
            Dictionary<string, string> dir = new Dictionary<string, string>();
            dir["filename"] = fileInfo.FullName;

            var fileUrl = await tusClient.Create(fileInfo, dir);
            var uploadResult = await tusClient.Upload(fileUrl, fileInfo);

```

* You can see more examples in unit tests

## Document

[Wiki](https://github.com/bluetianx/BirdMessenger/wiki)

## Roadmap

I will develop in branch of dev

## Who is using

* [China National Petroleum Corporation](https://www.cnpc.com.cn/cnpc/index.shtml)
## Support and Sponsorship

<a href="https://www.jetbrains.com" target="_blank">
    <img src="./docs/img/jetbrains_logo.png" title="JetBrains" width="100" />
</a>