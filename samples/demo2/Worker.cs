using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace demo2
{
    public class Worker : BackgroundService
    {
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
                FileInfo fileInfo = new FileInfo("test.txt");
                //var url = await _tusClient.Create(fileInfo);
                //await _tusClient.Upload(url, fileInfo, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exec async");
            }
        }
    }
}
