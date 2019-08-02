# BirdMessenger

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

``Install-Package BirdMessenger -Version 1.0.0-beta1``

.NET CLI

``dotnet add package BirdMessenger --version 1.0.0-beta1``

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

## RoadMap

I will develop in branch of dev

## Who is using

* [China National Petroleum Corporation](https://www.cnpc.com.cn/cnpc/index.shtml)