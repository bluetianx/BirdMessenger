using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace BirdMessenger.Test;

public class TusDownloadTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    public static Uri TusEndpoint = new Uri("http://localhost:5094/files");

    public TusDownloadTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("http://localhost:5094/files/fakefile")]
    public async Task TusDownloadExceptionTest(string fileUrl)
    {
        using var httpClient = new HttpClient();
        Exception ex = null;

        TusDownloadRequestOption downloadRequestOption = new TusDownloadRequestOption
        {
            FileLocation = new Uri(fileUrl),
            OutputStream = new MemoryStream(),
            OnPreSendRequestAsync = x => Task.CompletedTask,
            OnProgressAsync = x => Task.CompletedTask,
            OnCompletedAsync = x => Task.CompletedTask,
            OnFailedAsync = x =>
            {
                ex = x.Exception;
                _testOutputHelper.WriteLine($"error: {x.Exception.Message}");
                return Task.CompletedTask;
            }
        };

        var tusDownloadResp = await httpClient.TusDownloadAsync(downloadRequestOption, CancellationToken.None);

        Assert.NotNull(ex);
        Assert.IsType<TusException>(ex);

        var tusException = ex as TusException;

        Assert.NotNull(tusException.OriginHttpRequest);
        Assert.NotNull(tusException.OriginHttpResponse);
    }
}
