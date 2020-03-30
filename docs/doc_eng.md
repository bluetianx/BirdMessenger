### Get Start
```C#
            FileInfo fileInfo = new FileInfo("testfile");//a file which will upload 
            
            var hostUri = new Uri(@"http://localhost:5000/files");// tus server Url
            //Build default tusclient
            var tusClient=TusBuild.DefaultTusClientBuild(hostUri)
                .Build();
            //subscribe event
            tusClient.Uploading += printUploadProcess;
            tusClient.UploadFinish += UploadFinish;
            // create Upload-Metadata
            Dictionary<string, string> dir = new Dictionary<string, string>();
            dir["filename"] = fileInfo.FullName;
            //create a Url of uploadfile
            var fileUrl = await tusClient.Create(fileInfo, dir);
            //start upload
            var uploadResult = await tusClient.Upload(fileUrl, fileInfo);
```
### subscribe event of upload
TusClient has two events 

```C#
        public event Action<TusUploadContext> UploadFinish;

        /// <summary>
        /// uri  offset fileLength 
        /// </summary>
        public event Action<TusUploadContext> Uploading; 
```

* Action<TusUploadContext> UploadFinish: It can be invoked when the file has been uploaded
* Action<TusUploadContext> Uploading:This method is used to notify the progress of the upload

## TusUploadContext specification

The field definition for TusUploadContext is as follows：

* public   long TotalSize { get; }          :Total Size of upload File
* public   long UploadedSize { get; set; }  : size per transfer
* public  FileInfo UploadFileInfo { get; }  : upload file info
* public  Uri UploadFileUrl { get;}         : URl of upload file

###  build Tusclient by TusBuild

## Default Build Method
```C#
var tusClient=TusBuild.DefaultTusClientBuild(tusHost)
                .Build();
```
* tusHost is  tus server Url，clientName default value is  tusClient ，By default, every HTTP request is given with a "Tus-Resumable", "1.0.0" request header (except for Options requests)

## Customization
```C#
 var tusClient = TusBuild.DefaultTusClientBuild(tusHost)
                .Configure(option =>
                {
                    option.GetUploadSize = (u, t) => 10 * 1024 * 1024;
                })
                .Build();
```
* TusBuild  has  Configure method for custom TusClient. option  is a TusClientOption Instance，It contains properties for custom configuration
* The implement of ITusCore,ITusExtension and  IHttpClientFactory Interface can be rewrite by setting  IServiceCollection Servces property of option
* custom size of every upload by rewriting GetUploadSize method
## TusBuild specification
1. public static TusBuild DefaultTusClientBuild(Uri tushost,string clientName="")
    - summary：default build method
    - first parameter  is tus server host
    - second parameter is  logical name of the http client to create with IHttpClientFactory
2. TusBuild Configure(Action<TusClientOption> configAction)
   - summary：to configure TusClient
3. TusClientOption specification
   - public Uri TusHost { get; set; } ：tus server host
   - public IServiceCollection Servces { get; set; } ：contain implement of ITusCore,ITusExtension and IHttpClientFactory
   - public Func<TusUploadContext, int> GetUploadSize = (context) => 1 * 1024 * 1024 ：custom size of every upload 



### Polly Integration 
[httpclientfactoryWithPolly](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0#use-polly-based-handlers)