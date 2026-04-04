using BirdMessenger;
using BirdMessenger.Collections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace demo2;

public sealed class Worker2 : BackgroundService
{
    private static readonly Uri TusEndpoint = new("http://localhost:5094/files");

    private readonly ILogger<Worker2> _logger;
    private readonly ITusClient _tusClient;

    public Worker2(ILogger<Worker2> logger, ITusClient tusClient)
    {
        _logger = logger;
        _tusClient = tusClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var fileInfo = new FileInfo(Path.Combine(AppContext.BaseDirectory, "test.txt"));
            using var fileStream = fileInfo.OpenRead();

            var createResponse = await _tusClient.TusCreateAsync(
                new TusCreateRequestOption
                {
                    Endpoint = TusEndpoint,
                    IsUploadDeferLength = true,
                    Metadata = CreateMetadata(fileInfo)
                },
                stoppingToken);

            _logger.LogInformation("Deferred upload created at {Location}", createResponse.FileLocation);

            var patchResponse = await _tusClient.TusPatchAsync(
                new TusPatchRequestOption
                {
                    FileLocation = createResponse.FileLocation,
                    Stream = fileStream,
                    IsUploadDeferLength = true,
                    UploadType = UploadType.Chunk,
                    UploadBufferSize = 128 * 1024,
                    OnProgressAsync = evt =>
                    {
                        _logger.LogInformation("Worker2 upload progress: {Uploaded}/{Total}", evt.UploadedSize, evt.TotalSize);
                        return Task.CompletedTask;
                    }
                },
                stoppingToken);

            _logger.LogInformation("Worker2 upload completed with {UploadedSize} bytes", patchResponse.UploadedSize);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker2 upload cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker2 upload failed");
        }
    }

    private static MetadataCollection CreateMetadata(FileInfo fileInfo)
    {
        var metadata = new MetadataCollection
        {
            ["filename"] = fileInfo.Name,
            ["source"] = "worker2"
        };

        return metadata;
    }
}
