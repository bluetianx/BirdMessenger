### 简单使用
```C#
            FileInfo fileInfo = new FileInfo("testfile");//待上传文件
            
            var hostUri = new Uri(@"http://localhost:5000/files");// tus 服务端地址
            //构建一个默认的tusclient
            var tusClient=TusBuild.DefaultTusClientBuild(hostUri)
                .Build();
            //订阅事件
            tusClient.Uploading += printUploadProcess;
            tusClient.UploadFinish += UploadFinish;
            //构建Upload-Metadata
            Dictionary<string, string> dir = new Dictionary<string, string>();
            dir["filename"] = fileInfo.FullName;
            //创建上传文件URl
            var fileUrl = await tusClient.Create(fileInfo, dir);
            //开始上传
            var uploadResult = await tusClient.Upload(fileUrl, fileInfo);
```
### 订阅上传事件
TusClient 类有两个事件

```C#
        public event Action<TusUploadContext> UploadFinish;

        /// <summary>
        /// uri  offset fileLength 
        /// </summary>
        public event Action<TusUploadContext> Uploading; 
```

* Action<TusUploadContext> UploadFinish: 这个方法是文件上传完成时会被调用
* Action<TusUploadContext> Uploading:这方法是用来通知上传进度

## TusUploadContext 说明

TusUploadContext中的字段定义如下：

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
* 默认构建的中，tusHost 为tus服务端地址，clientName默认值为 tusClient ，每次发生http请求默认会带上 "Tus-Resumable", "1.0.0" 请求头（除了Options请求）
## 自定义配置
```C#
 var tusClient = TusBuild.DefaultTusClientBuild(tusHost)
                .Configure(option =>
                {
                    option.GetUploadSize = (u, t) => 10 * 1024 * 1024;
                })
                .Build();
```
* TusBuild中的Configure 是用于自定义配置的方法，option  是一个TusClientOption 对象，里面包含了用于自定义配置的属性
* 可以通过option 中的 IServiceCollection Servces 字段自定义ITusCore,ITusExtension接口以及IHttpClientFactory接口的实现
* 通过自定义 option 中的 GetUploadSize 函数来自定义每次上传文件的大小
## TusBuild类 说明
1. public static TusBuild DefaultTusClientBuild(Uri tushost,string clientName="")
    - 简介：默认的构建方法
    - 第一个参数是tus服务端地址
    - 第二个参数是httpclient通过httpclientfactory获取httpclient的标识
2. TusBuild Configure(Action<TusClientOption> configAction)
   - 简介：用于配置TusClient
3. TusClientOption 说明
   - public Uri TusHost { get; set; } ：tus服务器端地址
   - public IServiceCollection Servces { get; set; } ：里面包含ITusCore,ITusExtension接口以及IHttpClientFactory接口的实现
   - public Func<TusUploadContext, int> GetUploadSize = (context) => 1 * 1024 * 1024 ：该方法用于配置每次文件传输的大小



### 集成Polly
[参考这篇httpclientfactory如何集成Polly](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0#use-polly-based-handlers)