using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Collections;
using BirdMessenger.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace BirdMessenger.Test;

public class TusPatchTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    public static Uri TusEndpoint = new Uri("http://localhost:5094/files");
    
    public TusPatchTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task TusExcepitonTest()
    {
        using var httpClient = new HttpClient();
        var fileInfo = new FileInfo(@"TestFile/test1");
        var fileStream = new FileStream(fileInfo.FullName,FileMode.Open,FileAccess.Read);
        
        Exception ex = null;        

        TusPatchRequestOption tusPatchRequestOption = new TusPatchRequestOption
        {
            FileLocation = TusEndpoint,
            Stream = fileStream,
            OnPreSendRequestAsync = x =>
            {
                
                return Task.CompletedTask;
            },
            OnProgressAsync = x =>
            {
                return Task.CompletedTask;
            },
            OnCompletedAsync = x =>
            {
                return Task.CompletedTask;
            },
            OnFailedAsync = x =>
            {
                ex = x.Exception;
                _testOutputHelper.WriteLine($"error： {x.Exception.Message}");
                return Task.CompletedTask;
            }
        };

        var tusPatchResp = await httpClient.TusPatchAsync(tusPatchRequestOption, CancellationToken.None);
        
        Assert.NotNull(ex);
        Assert.IsType<TusException>(ex);
        
        var tusException = ex as TusException;
        
        Assert.NotNull(tusException.OriginHttpRequest);
        Assert.NotNull(tusException.OriginHttpResponse);
    }
}