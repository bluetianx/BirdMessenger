using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Collections;
using Xunit;
using Xunit.Abstractions;

namespace BirdMessenger.Test;

public class HttpClientExtensionTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    public static Uri TusEndpoint = new Uri("http://localhost:5094/files");

    public HttpClientExtensionTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    

    [Theory]
    [InlineData(1000)]
    [InlineData(50)]
    [InlineData(1)]
    [InlineData(0)]
    public async Task TestTusHeadAsync(long uploadLength)
    {
        using var httpClient = new HttpClient();
        TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
        {
            Endpoint = TusEndpoint,
            UploadLength = uploadLength,
            IsUploadDeferLength = uploadLength <= 0,
            OnPreSendRequestAsync = x =>
            {
                _testOutputHelper.WriteLine("OnPreSendRequestAsync is invoked");
                return Task.CompletedTask;
            }
        };
        var resp = await httpClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
        
        bool isInovkeOnPreSendRequestAsync = false;
        var tusHeadRequestOption = new TusHeadRequestOption
        {
            FileLocation = resp.FileLocation,
            OnPreSendRequestAsync = x =>
            {
                _testOutputHelper.WriteLine("TusHeadRequestOption is invoked");
                isInovkeOnPreSendRequestAsync = true;
                return Task.CompletedTask;
            }
        };

        var tusHeadResp = await httpClient.TusHeadAsync(tusHeadRequestOption, CancellationToken.None);

        Assert.True(isInovkeOnPreSendRequestAsync);
        Assert.Equal(0, tusHeadResp.UploadOffset);
        if (uploadLength > 0)
        {
            Assert.Equal(uploadLength, tusHeadResp.UploadLength);
        }
        else
        {
            Assert.True(tusHeadResp.UploadLength < 0);
        }
        Assert.Equal(TusVersion.V1_0_0, tusHeadResp.TusResumableVersion);
    }

    #region TestPatch

    [Theory]
    [InlineData(UploadType.Stream,1*1024)]
    [InlineData(UploadType.Stream,1*1024*1024)]
    [InlineData(UploadType.Chunk,1*1024)]
    [InlineData(UploadType.Chunk,4*1024*1024)]
    [InlineData(UploadType.Chunk,1*1024*1024)]
    public async Task TestPatchAsync(UploadType uploadOption,uint bufferSize)
    {
        using var httpClient = new HttpClient();
        var fileInfo = new FileInfo(@"TestFile/test1");
        var fileStream = new FileStream(fileInfo.FullName,FileMode.Open,FileAccess.Read);
        MetadataCollection metadata = new MetadataCollection();
        metadata["filename"] = fileInfo.Name;
        TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
        {
            Endpoint = TusEndpoint,
            Metadata = metadata,
            UploadLength = fileStream.Length,
        };
        var resp = await httpClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
        bool isInvokeOnProgressAsync = false;
        bool isInvokeOnCompletedAsync = false;
        long uploadedSize = 0;
        bool isIncludedCustomHeader = true;
        string customHeader = "customHeader";
        string customHeaderValue = "customHeaderValue";
        bool isInvokeHeadMethod = false;

        TusPatchRequestOption tusPatchRequestOption = new TusPatchRequestOption
        {
            FileLocation = resp.FileLocation,
            Stream = fileStream,
            UploadType = uploadOption,
            UploadBufferSize = bufferSize,
            OnPreSendRequestAsync = x =>
            {
                x.HttpRequestMsg.Headers.TryGetValues(customHeader, out var values);
                if (values != null && values.Contains(customHeaderValue))
                {
                }
                else
                {
                    isIncludedCustomHeader = false;
                }

                if (!isInvokeHeadMethod)
                {
                    isInvokeHeadMethod = x.HttpRequestMsg.Method == HttpMethod.Head;
                }
                return Task.CompletedTask;
            },
            OnProgressAsync = x =>
            {
                isInvokeOnProgressAsync = true;
                uploadedSize = x.UploadedSize;
                var uploadedProgress = (int)Math.Floor(100 * (double)x.UploadedSize / x.TotalSize);
                _testOutputHelper.WriteLine($"OnProgressAsync-TotalSize:{x.TotalSize}-UploadedSize:{x.UploadedSize}-uploadedProgress:{uploadedProgress}");
                return Task.CompletedTask;
            },
            OnCompletedAsync = x =>
            {
                isInvokeOnCompletedAsync = true;
                var reqOption = x.TusRequestOption as TusPatchRequestOption;
                _testOutputHelper.WriteLine($"File:{reqOption.FileLocation} Completed ");
                return Task.CompletedTask;
            },
            OnFailedAsync = x =>
            {
                _testOutputHelper.WriteLine($"error： {x.Exception.Message}");
                return Task.CompletedTask;
            }
        };
        tusPatchRequestOption.HttpHeaders[customHeader] = customHeaderValue;

        var tusPatchResp = await httpClient.TusPatchAsync(tusPatchRequestOption, CancellationToken.None);
        
        Assert.True(isInvokeOnProgressAsync);
        Assert.True(isInvokeOnCompletedAsync);
        Assert.Equal(fileStream.Length,tusPatchResp.UploadedSize);
        Assert.True(isIncludedCustomHeader);
        Assert.True(isInvokeHeadMethod);
    }

    [Theory]
    [InlineData(UploadType.Stream,1*1024)]
    [InlineData(UploadType.Stream,1*1024*1024)]
    [InlineData(UploadType.Chunk,1*1024)]
    [InlineData(UploadType.Chunk,1*1024*1024)]
    public async Task TestResumeUploadAsync(UploadType uploadOption,uint bufferSize)
    {
        using var httpClient = new HttpClient();
        var fileInfo = new FileInfo(@"TestFile/test1");
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
        bool isInokeOnFailedAsync = false;
        long uploadedSize = 0;
        bool isIncludedCustomHeader = true;
        string customHeader = "customHeader";
        string customHeaderValue = "customHeaderValue";
        bool isInvokeHeadMethod = false;

        TusPatchRequestOption tusPatchRequestOption = new TusPatchRequestOption
        {
            FileLocation = resp.FileLocation,
            Stream = fileStream,
            UploadType = uploadOption,
            UploadBufferSize = bufferSize,
            OnPreSendRequestAsync = x =>
            {
                x.HttpRequestMsg.Headers.TryGetValues(customHeader, out var values);
                if (values != null && values.Contains(customHeaderValue))
                {
                }
                else
                {
                    isIncludedCustomHeader = false;
                }

                if (!isInvokeHeadMethod)
                {
                    isInvokeHeadMethod = x.HttpRequestMsg.Method == HttpMethod.Head;
                }
                return Task.CompletedTask;
            },
            OnProgressAsync = x =>
            {
                isInvokeOnProgressAsync = true;
                uploadedSize = x.UploadedSize;
                var uploadedProgress = (int)Math.Floor(100 * (double)x.UploadedSize / x.TotalSize);
                //_testOutputHelper.WriteLine($"OnProgressAsync-TotalSize:{x.TotalSize}-UploadedSize:{x.UploadedSize}-uploadedProgress:{uploadedProgress}");
                if (uploadedProgress > 50)
                {
                    throw new Exception("test Error");
                }
                return Task.CompletedTask;
            },
            OnCompletedAsync = x =>
            {
                isInvokeOnCompletedAsync = true;
                var reqOption = x.TusRequestOption as TusPatchRequestOption;
                //_testOutputHelper.WriteLine($"File:{reqOption.FileLocation} Completed ");
                return Task.CompletedTask;
            },
            OnFailedAsync = x =>
            {
                _testOutputHelper.WriteLine($"error： {x.Exception.Message}");
                isInokeOnFailedAsync = true;
                return Task.CompletedTask;
            }
        };
        tusPatchRequestOption.HttpHeaders[customHeader] = customHeaderValue;


        var tusPatchResp = await httpClient.TusPatchAsync(tusPatchRequestOption, CancellationToken.None);
        
        Assert.True(tusPatchResp.UploadedSize <= fileStream.Length);
        Assert.Equal(tusPatchResp.UploadedSize,tusPatchResp.UploadedSize);
        Assert.True(isInvokeOnProgressAsync);
        Assert.False(isInvokeOnCompletedAsync);
        Assert.True(isInokeOnFailedAsync);
        Assert.True(isIncludedCustomHeader);
        Assert.True(isInvokeHeadMethod);
        
        isIncludedCustomHeader = true;
        isInvokeHeadMethod = false;
        tusPatchRequestOption = new TusPatchRequestOption
        {
            FileLocation = resp.FileLocation,
            Stream = fileStream,
            OnPreSendRequestAsync = x =>
            {
                x.HttpRequestMsg.Headers.TryGetValues(customHeader, out var values);
                if (values != null && values.Contains(customHeaderValue))
                {
                }
                else
                {
                    isIncludedCustomHeader = false;
                }

                if (!isInvokeHeadMethod)
                {
                    isInvokeHeadMethod = x.HttpRequestMsg.Method == HttpMethod.Head;
                }
                return Task.CompletedTask;
            },
            OnProgressAsync = x =>
            {
                isInvokeOnProgressAsync = true;
                uploadedSize = x.UploadedSize;
                var uploadedProgress = (int)Math.Floor(100 * (double)x.UploadedSize / x.TotalSize);
                _testOutputHelper.WriteLine($"OnProgressAsync-TotalSize:{x.TotalSize}-UploadedSize:{x.UploadedSize}-uploadedProgress:{uploadedProgress}");
                
                return Task.CompletedTask;
            },
            
            OnCompletedAsync = x =>
            {
                isInvokeOnCompletedAsync = true;
                var reqOption = x.TusRequestOption as TusPatchRequestOption;
                _testOutputHelper.WriteLine($"File:{reqOption.FileLocation} Completed ");
                return Task.CompletedTask;
            },
            OnFailedAsync = x =>
            {
                _testOutputHelper.WriteLine($"error： {x.Exception.Message}");
                return Task.CompletedTask;
            }
        };
        tusPatchRequestOption.HttpHeaders[customHeader] = customHeaderValue;

        tusPatchResp = await httpClient.TusPatchAsync(tusPatchRequestOption, CancellationToken.None);
        
        Assert.True(isInvokeOnCompletedAsync);
        Assert.Equal(tusPatchResp.UploadedSize,fileStream.Length);
        Assert.True(isIncludedCustomHeader);
        Assert.True(isInvokeHeadMethod);
    }

    #endregion

    [Fact]
    public async Task TestTusOptionAsync()
    {
        using var httpClient = new HttpClient();
        TusOptionRequestOption tusOptionRequestOption = new TusOptionRequestOption()
        {
            Endpoint = TusEndpoint
        };
        var resp = await httpClient.TusOptionAsync(tusOptionRequestOption, CancellationToken.None);
        
        Assert.Equal(TusVersion.V1_0_0, resp.TusResumableVersion);
        Assert.True(resp.TusVersions.Contains("1.0.0"));
    }
}