using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BirdMessenger.Test;

public class HttpClientExtensionTest
{
    public static Uri TusEndpoint = new Uri("http://localhost:5094/files");
    [Fact]
    public async Task TestTusCreateAsync()
    {
        using var httpClient = new HttpClient();
        TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
        {
            Endpoint = TusEndpoint,
            UploadLength = 1000
        };
        var resp =await httpClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
        Assert.Equal(TusVersion.V1_0_0,resp.TusVersion);
    }
}