using Microsoft.Extensions.Hosting;
using RaftCore.Adapters;
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

        public RaftService(IApplication exampleApplication)
        {
            _exampleApplication = exampleApplication;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask
                .Tee(_ => _exampleApplication.Initialise());

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask
                .Tee(_ => _exampleApplication.Deinitialise());

        public void Dispose()
        {}
    }
}
