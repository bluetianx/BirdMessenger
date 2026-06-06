### 简单使用

* 集成依赖注入

```C#
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
                    var uploadedProgress = x.TotalSize.HasValue ? (int)Math.Floor(100 * (double)x.UploadedSize / x.TotalSize.Value) : 0;
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
            // tusPatchResp.OriginResponseMessage
            // tusPatchResp.OriginHttpRequestMessage
```

* 延迟上传长度（Defer Length）

```c#
            // 使用 Defer Length 时不需要指定 UploadLength
            TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
            {
                Endpoint = TusEndpoint,
                IsUploadDeferLength = true
            };
            var resp = await tusClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);

            TusPatchRequestOption tusPatchRequestOption = new TusPatchRequestOption
            {
                FileLocation = resp.FileLocation,
                Stream = fileStream,
                IsUploadDeferLength = true,
                OnProgressAsync = x =>
                {
                    var uploadedSize = x.UploadedSize;
                    return Task.CompletedTask;
                }
            };
            var tusPatchResp = await tusClient.TusPatchAsync(tusPatchRequestOption, CancellationToken.None);
```

* 直接使用HttpClient的扩展方法

  ```c#
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
                      var uploadedProgress = x.TotalSize.HasValue ? (int)Math.Floor(100 * (double)x.UploadedSize / x.TotalSize.Value) : 0;
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
  ```

### API文档

参考 samples 文件夹下面的demo 实例 以及 单元测试项目的 测试用例，其中 ITusClient 调用方法与 Httpclient 扩展方法一致

#### TusCreateAsync

##### 定义

创建一个新的上传资源

```c#
Task<TusCreateResponse> TusCreateAsync(TusCreateRequestOption reqOption, CancellationToken ct = default)
```

##### 参数

###### TusCreateRequestOption

继承 [TusRequestOptionBase](#TusRequestOptionBase)

| 名称                | 类型                                                     | 说明                                                         |
| ------------------- | -------------------------------------------------------- | ------------------------------------------------------------ |
| Endpoint            | Uri                                                      | tus 服务器地址                                               |
| UploadLength        | long                                                     | 整个上传文件的大小（字节）                                   |
| IsUploadDeferLength | bool                                                     | 表示上传大小当前未知，将在后续传输                           |
| Metadata            | MetadataCollection, 实现 IDictionary<string, string>     | Upload-Metadata                                              |

##### 返回值

###### TusCreateResponse

继承 [TusResponseBase](#TusResponseBase)

| 名称         | 类型 | 说明              |
| ------------ | ---- | ----------------- |
| FileLocation | Uri  | 资源文件 URL      |

#### TusHeadAsync

##### 定义

tus HEAD 请求，获取上传偏移量

```c#
Task<TusHeadResponse> TusHeadAsync(TusHeadRequestOption reqOption, CancellationToken ct = default)
```

##### 参数

###### TusHeadRequestOption

继承 [TusRequestOptionBase](#TusRequestOptionBase)

| 名称         | 类型 | 说明          |
| ------------ | ---- | ------------- |
| FileLocation | Uri  | 上传文件 Uri  |

##### 返回值

###### TusHeadResponse

继承 [TusResponseBase](#TusResponseBase)

| 名称         | 类型 | 说明              |
| ------------ | ---- | ----------------- |
| UploadOffset | long | 上传偏移量        |
| UploadLength | long | 上传文件大小      |

#### TusPatchAsync

##### 定义

恢复上传文件

```c#
Task<TusPatchResponse> TusPatchAsync(TusPatchRequestOption reqOption, CancellationToken ct = default)
```

##### 参数

###### TusPatchRequestOption

继承 [TusRequestOptionBase](#TusRequestOptionBase)

| 名称                | 类型                             | 说明                                                         |
| ------------------- | -------------------------------- | ------------------------------------------------------------ |
| Stream              | Stream                           | 文件流                                                       |
| FileLocation        | Uri                              | 文件 Uri                                                     |
| UploadBufferSize    | uint                             | 上传大小，默认 1MB                                           |
| UploadType          | UploadType                       | 使用 Chunk 或 Stream 上传，默认 Stream                       |
| IsUploadDeferLength | bool                             | 表示上传大小当前未知，将在后续传输                           |
| OnProgressAsync     | Func<UploadProgressEvent,Task>?  | 上传进度回调                                                 |
| OnFailedAsync       | Func<UploadExceptionEvent,Task>? | 上传异常回调                                                 |
| OnCompletedAsync    | Func<UploadCompletedEvent,Task>? | 上传完成回调                                                 |

##### 返回值

###### TusPatchResponse

继承 [TusResponseBase](#TusResponseBase)

| 名称         | 类型 | 说明                   |
| ------------ | ---- | ---------------------- |
| UploadedSize | long | 已上传字节数           |

#### TusOptionAsync

##### 定义

获取 tus 服务器信息

```c#
Task<TusOptionResponse> TusOptionAsync(TusOptionRequestOption reqOption, CancellationToken ct)
```

##### 参数

###### TusOptionRequestOption

继承 [TusRequestOptionBase](#TusRequestOptionBase)

| 名称     | 类型 | 说明          |
| -------- | ---- | ------------- |
| Endpoint | Uri  | tus 服务器地址 |

##### 返回值

###### TusOptionResponse

继承 [TusResponseBase](#TusResponseBase)

| 名称          | 类型         | 说明               |
| ------------- | ------------ | ------------------ |
| TusVersions   | List<string> | 服务器支持的版本   |
| TusExtensions | List<string> | 服务器支持的扩展   |

#### TusDeleteAsync

##### 定义

删除文件

```c#
Task<TusDeleteResponse> TusDeleteAsync(TusDeleteRequestOption reqOption, CancellationToken ct)
```

##### 参数

###### TusDeleteRequestOption

继承 [TusRequestOptionBase](#TusRequestOptionBase)

| 名称         | 类型 | 说明      |
| ------------ | ---- | --------- |
| FileLocation | Uri  | 文件 Uri  |

##### 返回值

###### TusDeleteResponse

继承 [TusResponseBase](#TusResponseBase)

---

#### TusRequestOptionBase

##### 定义

Tus 请求基类

##### 属性

| 名称                  | 类型                                                     | 说明                         |
| --------------------- | -------------------------------------------------------- | ---------------------------- |
| OnPreSendRequestAsync | Func<[PreSendRequestEvent](#PreSendRequestEvent), Task>? | 发送 HTTP 请求前的回调       |
| HttpHeaders           | Dictionary<string,string>                                | 附加 HTTP 请求头             |

#### TusResponseBase

##### 定义

Tus 响应基类

##### 属性

| 名称                     | 类型                | 说明                |
| ------------------------ | ------------------- | ------------------- |
| OriginResponseMessage    | HttpResponseMessage | 原始 HTTP 响应      |
| OriginHttpRequestMessage | HttpRequestMessage  | 原始 HTTP 请求      |
| TusResumableVersion      | TusVersion          | 服务器 tus 版本     |

#### UploadEvent

##### 定义

Tus 上传事件抽象基类

##### 属性

| 名称             | 类型                                          | 说明 |
| ---------------- | --------------------------------------------- | ---- |
| TusRequestOption | [TusRequestOptionBase](#TusRequestOptionBase) |      |

#### PreSendRequestEvent

##### 定义

继承 UploadEvent

##### 属性

| 名称             | 类型                                          | 说明                              |
| ---------------- | --------------------------------------------- | --------------------------------- |
| TusRequestOption | [TusRequestOptionBase](#TusRequestOptionBase) |                                    |
| HttpRequestMsg   | HttpRequestMessage                            | 将发送到服务器的 HttpRequestMsg    |

#### UploadProgressEvent

##### 定义

继承 UploadEvent

##### 属性

| 名称         | 类型   | 说明                            |
| ------------ | ------ | ------------------------------- |
| TotalSize    | long?  | 整个上传大小（字节），未知时为 null |
| UploadedSize | long   | 已上传字节数                    |

#### UploadCompletedEvent

##### 定义

继承 UploadEvent

##### 属性

| 名称                | 类型                | 说明               |
| ------------------- | ------------------- | ------------------ |
| TusRequestOption    | TusRequestOptionBase |                    |
| OriginResponseMessage | HttpResponseMessage | 服务器原始响应     |

#### UploadExceptionEvent

##### 定义

继承 UploadEvent

##### 属性

| 名称                     | 类型                | 说明               |
| ------------------------ | ------------------- | ------------------ |
| TusRequestOption         | TusRequestOptionBase |                    |
| Exception                | Exception           | 上传过程中的异常   |
| OriginHttpResponse       | HttpResponseMessage | 原始 HTTP 响应     |
| OriginHttpRequestMessage | HttpRequestMessage  | 原始 HTTP 请求     |

#### TusException

##### 定义

继承 Exception

##### 属性

| 名称               | 类型                | 说明           |
| ------------------ | ------------------- | -------------- |
| OriginHttpRequest  | HttpRequestMessage  | 原始 HTTP 请求 |
| OriginHttpResponse | HttpResponseMessage | 原始 HTTP 响应 |





### 事件

### Event 
```C#
//每次发次http请求前会调用该事件
Func<PreSendRequestEvent,Task>? OnPreSendRequestAsync 

//文件上传进度
Func<UploadProgressEvent,Task>? OnProgressAsync
  

```




### 集成Polly
[参考这篇httpclientfactory如何集成Polly](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0#use-polly-based-handlers)
