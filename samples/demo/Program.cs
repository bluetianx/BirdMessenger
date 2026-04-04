using BirdMessenger;
using BirdMessenger.Collections;
using Microsoft.Extensions.DependencyInjection;

var sampleFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "test.txt"));
var endpoint = new Uri("http://localhost:5094/files");

Console.WriteLine($"Sample file: {sampleFile.FullName}");

await ShowServerCapabilitiesAsync(endpoint);
await UploadWithDependencyInjectionAsync(endpoint, sampleFile);
await UploadWithHttpClientAsync(endpoint, sampleFile);
await UploadWithDeferredLengthAsync(endpoint, sampleFile);

static async Task ShowServerCapabilitiesAsync(Uri endpoint)
{
    using var httpClient = new HttpClient();
    var optionResponse = await httpClient.TusOptionAsync(
        new TusOptionRequestOption
        {
            Endpoint = endpoint
        },
        CancellationToken.None);

    Console.WriteLine($"Server tus version: {optionResponse.TusResumableVersion}");
    Console.WriteLine($"Supported versions: {string.Join(", ", optionResponse.TusVersions)}");
    Console.WriteLine($"Supported extensions: {string.Join(", ", optionResponse.TusExtensions)}");
}

static async Task UploadWithDependencyInjectionAsync(Uri endpoint, FileInfo sampleFile)
{
    var services = new ServiceCollection();
    services.AddHttpClient<ITusClient, TusClient>();

    using var serviceProvider = services.BuildServiceProvider();
    var tusClient = serviceProvider.GetRequiredService<ITusClient>();

    using var stream = sampleFile.OpenRead();
    var createResponse = await tusClient.TusCreateAsync(
        new TusCreateRequestOption
        {
            Endpoint = endpoint,
            UploadLength = stream.Length,
            Metadata = BuildMetadata(sampleFile)
        },
        CancellationToken.None);

    Console.WriteLine($"DI upload location: {createResponse.FileLocation}");

    var patchResponse = await tusClient.TusPatchAsync(
        new TusPatchRequestOption
        {
            FileLocation = createResponse.FileLocation,
            Stream = stream,
            UploadType = UploadType.Stream,
            OnPreSendRequestAsync = evt =>
            {
                Console.WriteLine($"Sending {evt.HttpRequestMsg.Method} {evt.HttpRequestMsg.RequestUri}");
                return Task.CompletedTask;
            },
            OnProgressAsync = evt =>
            {
                Console.WriteLine(FormatProgress("DI", evt.UploadedSize, evt.TotalSize));
                return Task.CompletedTask;
            },
            OnCompletedAsync = evt =>
            {
                Console.WriteLine($"DI upload completed: {((TusPatchRequestOption)evt.TusRequestOption).FileLocation}");
                return Task.CompletedTask;
            },
            OnFailedAsync = evt =>
            {
                Console.WriteLine($"DI upload failed: {evt.Exception.Message}");
                return Task.CompletedTask;
            }
        },
        CancellationToken.None);

    Console.WriteLine($"DI uploaded bytes: {patchResponse.UploadedSize}");
}

static async Task UploadWithHttpClientAsync(Uri endpoint, FileInfo sampleFile)
{
    using var httpClient = new HttpClient();
    using var stream = sampleFile.OpenRead();

    var createResponse = await httpClient.TusCreateAsync(
        new TusCreateRequestOption
        {
            Endpoint = endpoint,
            UploadLength = stream.Length,
            Metadata = BuildMetadata(sampleFile)
        },
        CancellationToken.None);

    var headResponse = await httpClient.TusHeadAsync(
        new TusHeadRequestOption
        {
            FileLocation = createResponse.FileLocation
        },
        CancellationToken.None);

    Console.WriteLine($"Initial upload offset: {headResponse.UploadOffset}");

    var patchResponse = await httpClient.TusPatchAsync(
        new TusPatchRequestOption
        {
            FileLocation = createResponse.FileLocation,
            Stream = stream,
            UploadType = UploadType.Chunk,
            UploadBufferSize = 256 * 1024,
            OnProgressAsync = evt =>
            {
                Console.WriteLine(FormatProgress("HttpClient", evt.UploadedSize, evt.TotalSize));
                return Task.CompletedTask;
            },
            OnCompletedAsync = evt =>
            {
                Console.WriteLine($"HttpClient upload completed: {((TusPatchRequestOption)evt.TusRequestOption).FileLocation}");
                return Task.CompletedTask;
            },
            OnFailedAsync = evt =>
            {
                Console.WriteLine($"HttpClient upload failed: {evt.Exception.Message}");
                return Task.CompletedTask;
            }
        },
        CancellationToken.None);

    Console.WriteLine($"HttpClient uploaded bytes: {patchResponse.UploadedSize}");

    var deleteResponse = await httpClient.TusDeleteAsync(
        new TusDeleteRequestOption
        {
            FileLocation = createResponse.FileLocation
        },
        CancellationToken.None);

    Console.WriteLine($"Deleted upload with tus version: {deleteResponse.TusResumableVersion}");
}

static async Task UploadWithDeferredLengthAsync(Uri endpoint, FileInfo sampleFile)
{
    using var httpClient = new HttpClient();
    using var stream = sampleFile.OpenRead();

    var createResponse = await httpClient.TusCreateAsync(
        new TusCreateRequestOption
        {
            Endpoint = endpoint,
            IsUploadDeferLength = true,
            Metadata = BuildMetadata(sampleFile)
        },
        CancellationToken.None);

    var patchResponse = await httpClient.TusPatchAsync(
        new TusPatchRequestOption
        {
            FileLocation = createResponse.FileLocation,
            Stream = stream,
            IsUploadDeferLength = true,
            UploadType = UploadType.Stream,
            OnProgressAsync = evt =>
            {
                Console.WriteLine(FormatProgress("Deferred", evt.UploadedSize, evt.TotalSize));
                return Task.CompletedTask;
            }
        },
        CancellationToken.None);

    Console.WriteLine($"Deferred-length uploaded bytes: {patchResponse.UploadedSize}");
}

static MetadataCollection BuildMetadata(FileInfo fileInfo)
{
    var metadata = new MetadataCollection
    {
        ["filename"] = fileInfo.Name,
        ["contentType"] = "text/plain"
    };

    return metadata;
}

static string FormatProgress(string label, long uploadedSize, long? totalSize)
{
    if (!totalSize.HasValue || totalSize.Value <= 0)
    {
        return $"{label} progress: {uploadedSize} bytes";
    }

    var percent = (int)Math.Floor(100d * uploadedSize / totalSize.Value);
    return $"{label} progress: {uploadedSize}/{totalSize.Value} ({percent}%)";
}
