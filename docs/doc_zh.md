### 简单使用
```C#
            // file to be uploaded
FileInfo fileInfo = new FileInfo("test.txt");

// remote tus service
var hostUri = new Uri(@"http://localhost:5000/files");

// build a standalone tus client instance
var tusClient = TusBuild.DefaultTusClientBuild(hostUri).Build();

//hook up events
tusClient.UploadProgress += printUploadProcess;
tusClient.UploadFinish += uploadFinish;

//define additional file metadata 
MetadataCollection metadata = new MetadataCollection();
metadata["filename"] = fileInfo.FullName;

//create upload url
var fileUrl = await tusClient.Create(fileInfo, metadata);

//upload file
var uploadResult = await tusClient.Upload(fileUrl, fileInfo);

```
### 订阅上传事件
### Delegate 类型
```C#
/// <summary>
/// tus client delegate
/// </summary>
delegate void TusUploadDelegate(ITusClient source, ITusUploadContext tusUploadContext);
```
### ITusClient 事件
```C#
/// <summary>
/// upload completition event
/// </summary>
event TusUploadDelegate UploadFinish;

/// <summary>
/// upload progress event
/// </summary>
event TusUploadDelegate UploadProgress;
```

* UploadFinish: 文件上传完成时候会被调用
* UploadProgress: 用于通知上传进度


## ITusUploadContext 说明

ITusUploadContext中的字段定义如下：

* public   long TotalSize { get; }          :文件上传的总大小
* public   long UploadedSize { get; set; }  :文件每次传输大小
* public  FileInfo UploadFileInfo { get; }  : 上传文件的信息
* public  Uri UploadFileUrl { get;}         :上传文件对应的URl

### 利用TusBuild构建Tusclient

## 默认构建方法
```C#
var tusClient=TusBuild.DefaultTusClientBuild(tusHost)
                .Build();
```
###  利用DI构建tusclient实例

#### 通用的 ITusClient 配置
```C#
public static void ConfigureServices(IServiceCollection services)
{
    services.AddTusClient(tusHost);
}
```
```C#
public class Example
{
    public Example(ITusClient tusClient) 
    {
        ...
    }
}
```

#### 根据目标服务配置 ITusClient 
```C#
public static void ConfigureServices(IServiceCollection services)
{
    services.AddTusClient<Example>(tusHost);
}
```
```C#
public class Example
{
    public Example(ITusClient<Example> tusClient) 
    {
        ...
    }
}
```
* tusHost is tus 主机地址
* 每次发送http请求默认会带上 "Tus-Resumable", "1.0.0" 请求头（除了Options请求）

## ITusClient configuration

* 三个配置方法可以要么通过URI要么通过一个 configuration action 去调用

### 基本 configuration
```C#
Action<TusClientOptions> configure = (options) => {
    options.TusHost = tusHost;
    options.GetChunkUploadSize = (src, ctx) => 1 * 1024 * 1024; // 1 mega byte per upload request
    options.FileNameMetadataName = "fileName"; // default creation metadata
};
```
```C#
TusDefaultBuilder tusClientBuilder = TusBuild.DefaultTusClientBuild(configure);
```
OR
```C#
TusHttpClientBuilder tusClientBuilder = services.AddTusClient(configure);
```
OR
```C#
TusHttpClientBuilder tusClientBuilder = services.AddTusClient<Example>(configure);
```
### HttpClient 配置
```C#
var tusClientBuilder = TusBuild.DefaultTusClientBuild(configure);
// OR
// var tusClientBuilder = TusBuild.DefaultTusClientBuild(tusHost);

tusClientBuilder.Configure((TusClientOptions options, IHttpClientBuilder httpClientBuilder) => {
    //configure either options or httpClientBuilder
    //例如
    httpClientBuilder.ConfigureHttpClient(httpClient =>
                    {
                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", "ACCESS_TOKEN");
                    });
});
```
* TusHttpClientBuilder 主要有三个方法:
```C#
public TusHttpClientBuilder Configure(Action<TusClientOptions, IHttpClientBuilder> builder);
public TusHttpClientBuilder ConfigureCore(Action<TusClientOptions, IHttpClientBuilder> builder);
public TusHttpClientBuilder ConfigureExtension(Action<TusClientOptions, IHttpClientBuilder> builder);
```
* TusDefaultBuilder 继承 TusHttpClientBuilder
* Configure 会调用  ConfigureCore and ConfigureExtension 方法去配置HttpClient 
* 针对core 和扩展的tus 方法，可以使用不同的httplclient 配置，分别是 ConfigureCore与ConfigureExtension
### 集成Polly
[参考这篇httpclientfactory如何集成Polly](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0#use-polly-based-handlers)
