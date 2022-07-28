using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

    [Fact]
    public async Task TestTusCreateAsync()
    {
        bool isInovkeOnPreSendRequestAsync = false;
        using var httpClient = new HttpClient();
        TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
        {
            Endpoint = TusEndpoint,
            UploadLength = 1000,
            OnPreSendRequestAsync = x =>
            {
                _testOutputHelper.WriteLine("OnPreSendRequestAsync is invoked");
                isInovkeOnPreSendRequestAsync = true;
                return Task.CompletedTask;
            }
        };
        var resp = await httpClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
        Assert.Equal(TusVersion.V1_0_0, resp.TusVersion);
        Assert.True(isInovkeOnPreSendRequestAsync);
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

    [Theory]
    [InlineData(1000)]
    [InlineData(50)]
    [InlineData(-1)]
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
        Assert.Equal(TusVersion.V1_0_0, tusHeadResp.TusVersion);
    }
}