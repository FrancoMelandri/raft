using Microsoft.Extensions.Hosting;
using RaftApplication.Services.Application;
using System;
using System.Threading;
using System.Threading.Tasks;
using TinyFp.Extensions;

namespace RaftApplication.Services
{
    public class RaftService : IHostedService,
                               IDisposable
    {
        private readonly IExampleApplication _exampleApplication;

        public RaftService(IExampleApplication exampleApplication)
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
