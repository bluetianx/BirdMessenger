# Documentation
## Getting started
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
## Subscribe upload events
### Delegate prototype
```C#
/// <summary>
/// tus client delegate
/// </summary>
delegate void TusUploadDelegate(ITusClient source, ITusUploadContext tusUploadContext);
```
### ITusClient events 
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

* UploadFinish: It is invoked when the file has been uploaded
* UploadProgress: This method is used to notify the progress of the upload

## ITusUploadContext specification

The field definition for ITusUploadContext is as follows：

* public long TotalSize { get; }           : Total size of upload File
* public long UploadedSize { get; set; }   : Total uploaded size
* public FileInfo UploadFileInfo { get; }  : Upload file info
* public Uri UploadFileUrl { get;}         : URl of upload file

## Building an ITusClient instance
### Build standalone ITusClient by TusBuild.DefaultTusClientBuild
```C#
// returns an isolated instance of ITusClient
var tusClient = TusBuild.DefaultTusClientBuild(tusHost).Build(); 
```
###  Build ITusClient using Dependency Injection

#### Non-specific ITusClient configuration
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

#### Specific ITusClient configuration based on targeted service
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
* tusHost is tus server Url
* by default, every HTTP request is sent with a "Tus-Resumable: 1.0.0" header (except for Options requests)
## ITusClient configuration
* All three configuration methods can be called with either an URI or a configuration action

### Basic configuration
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
### HttpClient configuration
```C#
var tusClientBuilder = TusBuild.DefaultTusClientBuild(configure);
// OR
// var tusClientBuilder = TusBuild.DefaultTusClientBuild(tusHost);

tusClientBuilder.Configure((TusClientOptions options, IHttpClientBuilder httpClientBuilder) => {
    //configure either options or httpClientBuilder
});
```
* TusHttpClientBuilder has 3 main methods:
```C#
public TusHttpClientBuilder Configure(Action<TusClientOptions, IHttpClientBuilder> builder);
public TusHttpClientBuilder ConfigureCore(Action<TusClientOptions, IHttpClientBuilder> builder);
public TusHttpClientBuilder ConfigureExtension(Action<TusClientOptions, IHttpClientBuilder> builder);
```
* TusDefaultBuilder inherits TusHttpClientBuilder
* Configure will call both ConfigureCore and ConfigureExtension to configure the HttpClient used by either the Core of tus or the Extension of tus
* tus uses two different HttpClient for core and extensions, you may configure each as you need

### Polly Integration 
[HttpClientFactory With Polly](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0#use-polly-based-handlers)