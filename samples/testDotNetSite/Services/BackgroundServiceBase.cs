using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace testDotNetSite.Services
{
    /// <summary>
    /// BackgroundServiceBase
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.IHostedService" />
    /// <seealso cref="System.IDisposable" />
    public abstract class BackgroundServiceBase : IHostedService, IDisposable
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(CancellationToken)" /> is called.</param>
        /// <returns>
        /// A <see cref="Task" /> that represents the long running operations.
        /// </returns>
        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns></returns>
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {

            _executingTask = ExecuteAsync(_stoppingCts.Token);


            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }


            return Task.CompletedTask;
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns></returns>
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {

            if (_executingTask == null)
            {
                return;
            }

            try
            {

                _stoppingCts.Cancel();
            }
            finally
            {

                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }

        }

        public virtual void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }
}
