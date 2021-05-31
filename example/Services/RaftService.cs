using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TinyFp.Extensions;

namespace RaftApplication.Services
{
    public class RaftService : IHostedService,
                               IDisposable
    {
        private const string STARTED = "Raft hosted service started.";
        private const string STOPPED = "Raft hosted service stopped.";

        private readonly ILogger<RaftService> _logger;

        public RaftService(ILogger<RaftService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask.Tee(_ =>  _logger.LogInformation(STARTED));

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask.Tee(_ => _logger.LogInformation(STOPPED));

        public void Dispose()
        {}
    }
}
