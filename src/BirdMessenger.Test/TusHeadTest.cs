using System;
using System.Net.Http;
using System.Threading.Tasks;
using BirdMessenger.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace BirdMessenger.Test;

public class TusHeadTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    public static Uri TusEndpoint = new Uri("http://localhost:5094/files");

    public TusHeadTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("http://localhost:5094/files/fakefile")]
    public async Task TusExcepitonTest(string fileUrl)
    {
        using var httpClient = new HttpClient();

        Exception ex = null;

        try
        {
            TusHeadRequestOption tusHeadRequestOption = new TusHeadRequestOption()
            {
                FileLocation = new Uri(fileUrl),
            };
            await httpClient.TusHeadAsync(tusHeadRequestOption);
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.Message);
            ex = e;
        }
        
        Assert.NotNull(ex);
        Assert.IsType<TusException>(ex);
        
        var tusException = ex as TusException;
        
        Assert.NotNull(tusException.OriginHttpRequest);
        Assert.NotNull(tusException.OriginHttpResponse);
    }
    
}