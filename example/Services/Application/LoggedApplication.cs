using Microsoft.Extensions.Logging;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;

namespace RaftApplication.Services.Application
{
    public class LoggedApplication : IExampleApplication
    {
        private const string STARTED = "Raft application started.";
        private const string STOPPED = "Raft application stopped.";
        private const string MESSAGE = "Raft application message: ";

        private readonly IExampleApplication _exampleApplication;
        private readonly ILogger<LoggedApplication> _logger;

        public LoggedApplication(IExampleApplication exampleApplication,
                                 ILogger<LoggedApplication> logger)
        {
            _exampleApplication = exampleApplication;
            _logger = logger;
        }

        public Unit Deinitialise()
            => _exampleApplication
                .Initialise()
                .Tee(_ => _logger.LogInformation(STOPPED));

        public Unit Initialise()
            => _exampleApplication
                .Deinitialise()
                .Tee(_ => _logger.LogInformation(STARTED));

        public Unit NotifyMessage(Message message)
            => _exampleApplication
                .NotifyMessage(message)
                .Tee(_ => _logger.LogInformation($"{MESSAGE} {message.Type}"));
    }
}
