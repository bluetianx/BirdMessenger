using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Collections;
using Xunit;
using Xunit.Abstractions;

namespace BirdMessenger.Test;

public class TusCreateTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    public static Uri TusEndpoint = new Uri("http://localhost:5094/files");
    
    public TusCreateTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task CreateZoreSizeFile()
    {
        bool isInovkeOnPreSendRequestAsync = false;

        using var httpClient = new HttpClient();
        
        MetadataCollection dir = new MetadataCollection();
        dir["filename"] = "fileNameTest";
        TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
        {
            Endpoint = TusEndpoint,
            UploadLength = 0,
            Metadata = dir,
            OnPreSendRequestAsync = x =>
            {
                _testOutputHelper.WriteLine("OnPreSendRequestAsync is invoked");
                isInovkeOnPreSendRequestAsync = true;
                return Task.CompletedTask;
            }
        };
        var resp = await httpClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
        Assert.Equal(TusVersion.V1_0_0, resp.TusResumableVersion);
        Assert.True(isInovkeOnPreSendRequestAsync);
    }
    [Fact]
    public async Task CreateWithIsUploadDeferLength()
    {
        bool isInovkeOnPreSendRequestAsync = false;

        using var httpClient = new HttpClient();
        
        MetadataCollection dir = new MetadataCollection();
        dir["filename"] = "fileNameTest";
        TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
        {
            Endpoint = TusEndpoint,
            UploadLength = 0,
            IsUploadDeferLength = true,
            Metadata = dir,
            OnPreSendRequestAsync = x =>
            {
                _testOutputHelper.WriteLine("OnPreSendRequestAsync is invoked");
                isInovkeOnPreSendRequestAsync = true;
                return Task.CompletedTask;
            }
        };
        var resp = await httpClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
        Assert.Equal(TusVersion.V1_0_0, resp.TusResumableVersion);
        Assert.True(isInovkeOnPreSendRequestAsync);
    }
    [Fact]
    public async Task TestTusCreateAndDelAsync()
    {
        bool isInovkeOnPreSendRequestAsync = false;
        using var httpClient = new HttpClient();
        
        MetadataCollection dir = new MetadataCollection();
        dir["filename"] = "fileNameTest";
        TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
        {
            Endpoint = TusEndpoint,
            UploadLength = 1000,
            Metadata = dir,
            OnPreSendRequestAsync = x =>
            {
                _testOutputHelper.WriteLine("OnPreSendRequestAsync is invoked");
                isInovkeOnPreSendRequestAsync = true;
                return Task.CompletedTask;
            }
        };
        var resp = await httpClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
        Assert.Equal(TusVersion.V1_0_0, resp.TusResumableVersion);
        Assert.True(isInovkeOnPreSendRequestAsync);

        TusDeleteRequestOption tusDeleteRequestOption = new TusDeleteRequestOption()
        {
            FileLocation = resp.FileLocation
        };

        var delResp = await httpClient.TusDeleteAsync(tusDeleteRequestOption, CancellationToken.None);
        Assert.Equal(TusVersion.V1_0_0, delResp.TusResumableVersion);
    }

    [Fact]
    public async Task TestTusCreateArgumentExceptionAsync()
    {
        bool isArgumentException = false;
        try
        {
            using var httpClient = new HttpClient();
            TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
            {
                Endpoint = TusEndpoint,
                UploadLength = 1000,
                IsUploadDeferLength = true
            };
            var resp = await httpClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
        }
        catch (ArgumentException)
        {
            isArgumentException = true;
        }

        Assert.True(isArgumentException);
    }

}