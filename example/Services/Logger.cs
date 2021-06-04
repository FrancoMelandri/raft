using Microsoft.Extensions.Logging;
using TinyFp;
using TinyFp.Extensions;

namespace RaftApplication.Services
{
    public class Logger : RaftCore.Adapters.ILogger
    {
        private readonly ILogger<Logger> _logger;

        public Logger(ILogger<Logger> logger)
        {
            _logger = logger;
        }

        public Unit Error(string message)
            => Unit.Default
                .Tee(_ => _logger.LogError(message));

        public Unit Information(string message)
            => Unit.Default
                .Tee(_ => _logger.LogInformation(message));
    }
}
