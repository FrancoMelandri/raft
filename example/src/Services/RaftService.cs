using Microsoft.Extensions.Hosting;
using RaftCore.Adapters;
using RaftCore.Cluster;
using System;
using System.Threading;
using System.Threading.Tasks;
using TinyFp.Extensions;

namespace RaftApplication.Services
{
    public class RaftService : IHostedService,
                               IDisposable
    {
        private readonly IApplication _exampleApplication;
        private readonly ILocalNode _localNode;

        public RaftService(IApplication exampleApplication,
                           ILocalNode localNode)
        {
            _exampleApplication = exampleApplication;
            _localNode = localNode;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask
                .Tee(_ => _exampleApplication.Initialise(_localNode));

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask
                .Tee(_ => _exampleApplication.Deinitialise(_localNode));

        public void Dispose()
        {}
    }
}
