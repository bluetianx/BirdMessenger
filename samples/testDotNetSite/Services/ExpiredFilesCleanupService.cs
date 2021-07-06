using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;

namespace testDotNetSite.Services
{
    public class ExpiredFilesCleanupService : BackgroundServiceBase
    {
        private readonly ITusExpirationStore _expirationStore;
        private readonly ExpirationBase _expiration;
        private readonly ILogger<ExpiredFilesCleanupService> _logger;

        public ExpiredFilesCleanupService(ILogger<ExpiredFilesCleanupService> logger, DefaultTusConfiguration config)
        {
            _logger = logger;
            _expirationStore = (ITusExpirationStore)config.Store;
            _expiration = config.Expiration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_expiration == null)
            {
                _logger.LogInformation("Not running cleanup job as no expiration has been set.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunCleanup(stoppingToken);
                await Task.Delay(_expiration.Timeout);
            }
        }

        private async Task RunCleanup(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running cleanup job...");
            var numberOfRemovedFiles = await _expirationStore.RemoveExpiredFilesAsync(cancellationToken);
            _logger.LogInformation(
                $"Removed {numberOfRemovedFiles} expired files. Scheduled to run again in {_expiration.Timeout.TotalMilliseconds} ms");
        }
    }
}