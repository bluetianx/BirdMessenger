using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger;
using BirdMessenger.Collections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace demo2
{
    public class Worker2 : BackgroundService
    {
        private readonly ILogger<Worker2> _logger;
        private readonly ITusClient _tusClient;
        public static Uri TusEndpoint = new Uri("http://localhost:5094/files");

        public Worker2(ILogger<Worker2> logger, ITusClient tusClient)
        {
            _logger = logger;
            _tusClient = tusClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                FileInfo fileInfo = new FileInfo("test.txt");
                MetadataCollection metadata = new MetadataCollection();
                metadata["filename"] = fileInfo.Name;
                TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
                {
                    Endpoint = TusEndpoint,
                    Metadata = metadata,
                    UploadLength = fileInfo.Length
                };
                var tusCreateResp = await _tusClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
                using var fileStream = new FileStream(fileInfo.FullName,FileMode.Open,FileAccess.Read);
                TusPatchRequestOption tusPatchRequestOption = new TusPatchRequestOption()
                {
                    FileLocation = tusCreateResp.FileLocation,
                    Stream = fileStream
                };
                var tusPatchResp = await _tusClient.TusPatchAsync(tusPatchRequestOption, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exec async");
            }
        }
    }
}
