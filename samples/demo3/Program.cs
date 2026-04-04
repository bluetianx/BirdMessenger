using System.Text;
using BirdMessenger;
using BirdMessenger.Collections;

var endpoint = new Uri("http://localhost:5094/files");
using var httpClient = new HttpClient();
using var stream = CreateSampleStream(4 * 1024 * 1024);

var createResponse = await httpClient.TusCreateAsync(
    new TusCreateRequestOption
    {
        Endpoint = endpoint,
        UploadLength = stream.Length,
        Metadata = new MetadataCollection
        {
            ["filename"] = "memory-stream.txt",
            ["contentType"] = "text/plain"
        }
    },
    CancellationToken.None);

var patchResponse = await httpClient.TusPatchAsync(
    new TusPatchRequestOption
    {
        FileLocation = createResponse.FileLocation,
        Stream = stream,
        UploadType = UploadType.Stream,
        OnProgressAsync = evt =>
        {
            Console.WriteLine($"Memory upload progress: {evt.UploadedSize}/{evt.TotalSize}");
            return Task.CompletedTask;
        },
        OnCompletedAsync = evt =>
        {
            Console.WriteLine($"Memory upload completed: {((TusPatchRequestOption)evt.TusRequestOption).FileLocation}");
            return Task.CompletedTask;
        },
        OnFailedAsync = evt =>
        {
            Console.WriteLine($"Memory upload failed: {evt.Exception.Message}");
            return Task.CompletedTask;
        }
    },
    CancellationToken.None);

var headResponse = await httpClient.TusHeadAsync(
    new TusHeadRequestOption
    {
        FileLocation = createResponse.FileLocation
    },
    CancellationToken.None);

Console.WriteLine($"Uploaded {patchResponse.UploadedSize} bytes");
Console.WriteLine($"Remote upload offset: {headResponse.UploadOffset}");

static MemoryStream CreateSampleStream(int sizeInBytes)
{
    var stream = new MemoryStream(sizeInBytes);
    var line = Encoding.UTF8.GetBytes("BirdMessenger sample payload\n");

    while (stream.Length + line.Length <= sizeInBytes)
    {
        stream.Write(line, 0, line.Length);
    }

    if (stream.Length < sizeInBytes)
    {
        var remaining = sizeInBytes - (int)stream.Length;
        stream.Write(line, 0, remaining);
    }

    stream.Position = 0;
    return stream;
}
