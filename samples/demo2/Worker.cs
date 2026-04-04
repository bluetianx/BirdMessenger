using BirdMessenger;
using BirdMessenger.Collections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace demo2;

public sealed class Worker : BackgroundService
{
    private static readonly Uri TusEndpoint = new("http://localhost:5094/files");

    private readonly ILogger<Worker> _logger;
    private readonly ITusClient _tusClient;

    public Worker(ILogger<Worker> logger, ITusClient tusClient)
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
                    UploadLength = fileStream.Length,
                    Metadata = CreateMetadata(fileInfo)
                },
                stoppingToken);

            _logger.LogInformation("Standard upload created at {Location}", createResponse.FileLocation);

            var patchResponse = await _tusClient.TusPatchAsync(
                new TusPatchRequestOption
                {
                    FileLocation = createResponse.FileLocation,
                    Stream = fileStream,
                    UploadType = UploadType.Stream,
                    OnProgressAsync = evt =>
                    {
                        _logger.LogInformation("Worker upload progress: {Uploaded}/{Total}", evt.UploadedSize, evt.TotalSize);
                        return Task.CompletedTask;
                    }
                },
                stoppingToken);

            _logger.LogInformation("Worker upload completed with {UploadedSize} bytes", patchResponse.UploadedSize);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker upload cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker upload failed");
        }
    }

    private static MetadataCollection CreateMetadata(FileInfo fileInfo)
    {
        var metadata = new MetadataCollection
        {
            ["filename"] = fileInfo.Name,
            ["source"] = "worker"
        };

        return metadata;
    }
}
