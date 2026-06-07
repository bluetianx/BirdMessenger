using System;
using System.Net.Http;
using System.Threading.Tasks;
using BirdMessenger.Infrastructure;
using Xunit;

namespace BirdMessenger.Test;

public class ParameterValidationTest
{
    [Fact]
    public void TusClientConstructor_ThrowsArgumentNullException_WhenHttpClientIsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new TusClient(null));
        Assert.Equal("httpClient", exception.ParamName);
    }

    [Fact]
    public async Task TusCreateAsync_ThrowsArgumentException_WhenReqOptionIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            httpClient.TusCreateAsync(null, default));
        Assert.Contains("tusCreateRequestOption", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TusCreateAsync_ThrowsArgumentException_WhenEndpointIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            httpClient.TusCreateAsync(new TusCreateRequestOption { Endpoint = null }, default));
        Assert.Contains("Endpoint", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TusHeadAsync_ThrowsArgumentNullException_WhenReqOptionIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusHeadAsync(null, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusHeadAsync_ThrowsArgumentNullException_WhenFileLocationIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusHeadAsync(new TusHeadRequestOption { FileLocation = null }, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusPatchAsync_ThrowsArgumentNullException_WhenReqOptionIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusPatchAsync(null, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusPatchAsync_ThrowsArgumentNullException_WhenFileLocationIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusPatchAsync(new TusPatchRequestOption { FileLocation = null }, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusPatchAsync_ThrowsArgumentNullException_WhenStreamIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusPatchAsync(new TusPatchRequestOption
            {
                FileLocation = new Uri("http://localhost/files"),
                Stream = null
            }, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusPatchAsync_ThrowsArgumentOutOfRangeException_WhenUploadBufferSizeIsZero()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            httpClient.TusPatchAsync(new TusPatchRequestOption
            {
                FileLocation = new Uri("http://localhost/files"),
                Stream = new System.IO.MemoryStream(),
                UploadBufferSize = 0
            }, default));
        Assert.Equal("UploadBufferSize", exception.ParamName);
    }

    [Fact]
    public async Task TusPatchAsync_ThrowsArgumentOutOfRangeException_WhenUploadBufferSizeIsNegative()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            httpClient.TusPatchAsync(new TusPatchRequestOption
            {
                FileLocation = new Uri("http://localhost/files"),
                Stream = new System.IO.MemoryStream(),
                UploadBufferSize = unchecked((uint)(-1))
            }, default));
        Assert.Equal("UploadBufferSize", exception.ParamName);
    }

    [Fact]
    public async Task TusOptionAsync_ThrowsArgumentNullException_WhenReqOptionIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusOptionAsync(null, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusOptionAsync_ThrowsArgumentNullException_WhenEndpointIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusOptionAsync(new TusOptionRequestOption { Endpoint = null }, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusDeleteAsync_ThrowsArgumentNullException_WhenReqOptionIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusDeleteAsync(null, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusDeleteAsync_ThrowsArgumentNullException_WhenFileLocationIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusDeleteAsync(new TusDeleteRequestOption { FileLocation = null }, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusDownloadAsync_ThrowsArgumentNullException_WhenReqOptionIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusDownloadAsync(null, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusDownloadAsync_ThrowsArgumentNullException_WhenFileLocationIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusDownloadAsync(new TusDownloadRequestOption { FileLocation = null }, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusDownloadAsync_ThrowsArgumentNullException_WhenOutputStreamIsNull()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            httpClient.TusDownloadAsync(new TusDownloadRequestOption
            {
                FileLocation = new Uri("http://localhost/files"),
                OutputStream = null
            }, default));
        Assert.Equal("reqOption", exception.ParamName);
    }

    [Fact]
    public async Task TusDownloadAsync_ThrowsArgumentOutOfRangeException_WhenDownloadBufferSizeIsZero()
    {
        using var httpClient = new HttpClient();
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            httpClient.TusDownloadAsync(new TusDownloadRequestOption
            {
                FileLocation = new Uri("http://localhost/files"),
                OutputStream = new System.IO.MemoryStream(),
                DownloadBufferSize = 0
            }, default));
        Assert.Equal("DownloadBufferSize", exception.ParamName);
    }
}
