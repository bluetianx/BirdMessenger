# Documentation
## Getting started

* Integrate DI

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
* Using HttpClient Extension Methods directly

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

Reference demo in sample or unit test case in Unit test


### Polly Integration 
[HttpClientFactory With Polly](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0#use-polly-based-handlers)

### API Doc



#### TusCreateAsync



##### Definition

create a new upload resource

``` c#
Task<TusCreateResponse> TusCreateAsync(TusCreateRequestOption reqOption, CancellationToken ct = default)
```



##### Type Parameters

###### TusCreateRequestOption

[Derived TusRequestOptionBase](#TusRequestOptionBase)

| Name                | Type                                                     | Definition                                                   |
| ------------------- | -------------------------------------------------------- | ------------------------------------------------------------ |
| Endpoint            | Uri                                                      | tus server address                                           |
| UploadLength        | long                                                     | indicates the size of the entire upload in bytes             |
| IsUploadDeferLength | bool                                                     | indicates that the size of the upload is not known currently and will be transferred later |
| Metadata            | MetadataCollection,Implement IDictionary<string, string> | Upload-Metadata                                              |



##### Returns

###### TusCreateResponse

[Derived TusResponseBase](#TusResponseBase)

| Name         | Type | Definition        |
| ------------ | ---- | ----------------- |
| FileLocation | Uri  | resource file URL |



#### TusHeadAsync

##### Definition

tus Head request For getting upload-offset

```c#
Task<TusHeadResponse> TusHeadAsync(TusHeadRequestOption reqOption, CancellationToken ct = default)
```



##### Type Parameters

######  TusHeadRequestOption

[Derived TusRequestOptionBase](#TusRequestOptionBase)

| Name         | Type | Definition      |
| ------------ | ---- | --------------- |
| FileLocation | Uri  | Upload File Uri |



##### Returns

###### TusHeadResponse

[Derived TusResponseBase](#TusResponseBase)

| Name         | Type | Definition             |
| ------------ | ---- | ---------------------- |
| UploadOffset | long | Upload Offset          |
| UploadLength | long | the size of the upload |





#### TusPatchAsync

##### Definition

resume upload file

```c#
Task<TusPatchResponse> TusPatchAsync(TusPatchRequestOption reqOption, CancellationToken ct = default)
```



##### Type Parameters



###### TusPatchRequestOption

[Derived TusRequestOptionBase](#TusRequestOptionBase)

| Name             | Type                             | Definition                                                   |
| ---------------- | -------------------------------- | ------------------------------------------------------------ |
| Stream           | Stream                           | file stream                                                  |
| FileLocation     | Uri                              | file uri                                                     |
| UploadBufferSize | uint                             | uploadSize ,default value 1MB                                |
| UploadType       | UploadType                       | setting upload file with chunk or stream, default value is Stream |
| OnProgressAsync  | Func<UploadProgressEvent,Task>?  | invoke when uploading file                                   |
| OnFailedAsync    | Func<UploadExceptionEvent,Task>? | invoke when appear a Exception                               |
| OnCompletedAsync | Func<UploadCompletedEvent,Task>? | invoke when complete uploading                               |



##### Returns

###### TusPatchResponse

[Derived TusResponseBase](#TusResponseBase)

| Name         | Type | Definition                          |
| ------------ | ---- | ----------------------------------- |
| UploadedSize | Long | indicate the size of uploaded bytes |





#### TusOptionAsync

##### Definition

getting tusServer Info

```c#
Task<TusOptionResponse> TusOptionAsync( TusOptionRequestOption reqOption, CancellationToken ct)
```

##### Type Parameters

###### TusOptionRequestOption

[Derived TusRequestOptionBase](#TusRequestOptionBase)

| Name     | Type | Definition         |
| -------- | ---- | ------------------ |
| Endpoint | Uri  | tus server address |



##### Returns

###### TusOptionResponse

[Derived TusResponseBase](#TusResponseBase)

| Name          | Type         | Definition                 |
| ------------- | ------------ | -------------------------- |
| TusVersions   | List<string> | server supports versions   |
| TusExtensions | List<string> | server supports extensions |



#### TusDeleteAsync

##### Definition

delete  file

```c#
Task<TusDeleteResponse> TusDeleteAsync(TusDeleteRequestOption reqOption, CancellationToken ct)
```



##### Type Parameters

###### TusDeleteRequestOption

[Derived TusRequestOptionBase](#TusRequestOptionBase)

| Name         | Type | Definition |
| ------------ | ---- | ---------- |
| FileLocation | Uri  | file uri   |



##### Returns

###### TusDeleteResponse

[Derived TusRequestOptionBase](#TusRequestOptionBase)





---



#### TusRequestOptionBase

##### Definition

Responsible TusRequest Base Class



##### Properties

| Name                  | Type                                                     | Definition                                     |
| --------------------- | -------------------------------------------------------- | ---------------------------------------------- |
| OnPreSendRequestAsync | Func<[PreSendRequestEvent](#PreSendRequestEvent), Task>? | invoke before sending HTTP request to a server |
| HttpHeaders           | Dictionary<string,string>                                | add additional HTTP headers while HTTP request |
|                       |                                                          |                                                |



#### TusResponseBase

##### Definition

Responsible TusResponse Base Class



##### Properties

| Name                     | Type                | Definition                |
| ------------------------ | ------------------- | ------------------------- |
| OriginResponseMessage    | HttpResponseMessage | origin http response      |
| OriginHttpRequestMessage | HttpRequestMessage  | origin HttpRequestMessage |
| TusResumableVersion      | TusVersion          | tus version from server   |



#### UploadEvent

##### Definition

Tus Upload file Abstract Class event



##### Properties

| Name             | Type                                          | Definition |
| ---------------- | --------------------------------------------- | ---------- |
| TusRequestOption | [TusRequestOptionBase](#TusRequestOptionBase) |            |



#### PreSendRequestEvent

##### Definition

Derived UploadEvent

##### Properties

| Name             | Type                                          | Definition                              |
| ---------------- | --------------------------------------------- | --------------------------------------- |
| TusRequestOption | [TusRequestOptionBase](#TusRequestOptionBase) |                                         |
| HttpRequestMsg   | HttpRequestMessage                            | A HttpRequestMsg will be send to server |



#### TusException

##### Definition

Derived Exception

##### Properties

| Name               | Type                | Definition           |
| ------------------ | ------------------- | -------------------- |
| OriginHttpRequest  | HttpRequestMessage  | origin http request  |
| OriginHttpResponse | HttpResponseMessage | origin http response |