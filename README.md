# BirdMessenger

>"Our aim is to solve the problem of unreliable file uploads once and for all. tus is a new open protocol for resumable uploads built on HTTP. It offers simple, cheap and reusable stacks for clients and servers. It supports any language, any platform and any network." - https://tus.io

BirdMessenger 中文名为：青鸟——相传为西王母的信使。
BirdMessnger 是一个基于.NET Standard 的 Tus协议的实现客户端。

## Features

### 协议实现

* Create
* HEAD
* PATCH
* OPTIONS

## Install

Package manager

``PM> Install-Package BirdMessenger -Version 0.1.4``

.NET CLI

``> dotnet add package BirdMessenger --version 0.1.4``

## Getting Started

```C#

FileInfo fileInfo = new FileInfo("test.dmg");
            UploadConfig uploadConfig =new UploadConfig();
            uploadConfig.ServerUrl = new Uri(@"http://localhost:1080/uploads");
            uploadConfig.UploadFile= fileInfo;
            uploadConfig.Uploading=printUploadProcess;
            uploadConfig.PreCreateRequest=preCreateFile;
            uploadConfig.PreUploadRequest= preUploadFile;
            uploadConfig.UploadFinish=UploadFinish;
            TusClient  tusClient = new TusClient(uploadConfig);

            var url = tusClient.Create();

            tusClient.UploadFile();
```

* 详细细节可以查看samples文件夹下的示例代码

## 谁在使用

* [中国石油](https://www.cnpc.com.cn/cnpc/index.shtml)