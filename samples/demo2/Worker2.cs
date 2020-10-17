using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace demo2
{
    public class Worker2 : BackgroundService
    {
        private readonly ILogger<Worker2> _logger;
        private readonly ITusClient<Worker2> _tusClient;

        public Worker2(ILogger<Worker2> logger, ITusClient<Worker2> tusClient)
        {
            _logger = logger;
            _tusClient = tusClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                FileInfo fileInfo = new FileInfo("test.txt");
                var url = await _tusClient.Create(fileInfo);
                await _tusClient.Upload(url, fileInfo, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exec async");
            }
        }
    }
}
