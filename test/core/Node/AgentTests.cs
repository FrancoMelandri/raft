using NUnit.Framework;
using RaftCore;

namespace RaftTest.Core
{
    [TestFixture]
    public class AgentTests : BaseUseCases    
    {
        [Test]
        public void LogError_CallLogger()
        {
            _ = _sut.LogError(new Error("code", "description"));

            _logger
                .Verify(m => m.Error("code: description"));
        }

        [Test]
        public void LogInformation_CallLogger()
        {
            _ = _sut.LogInformation("information");

            _logger
                .Verify(m => m.Information("information"));
        }
    }
}
