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
            // tusPatchResp.OriginResponseMessage
            // tusPatchResp.OriginHttpRequestMessage
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
  ```

### API文档

参考 samples 文件夹下面的demo 实例 以及 单元测试项目的 测试用例，其中 ITusClient 调用方法与 Httpclient 扩展方法一致





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
