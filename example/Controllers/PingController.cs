using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TinyFp;
using TinyFp.Extensions;

namespace TinyFpTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PingController : ControllerBase
    {
        private const string PONG = "pong";
        private readonly ILogger<PingController> _logger;

        public PingController(ILogger<PingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Task<string> Get()
            => PONG
                .AsTask()
                .Tee(_ => _logger.LogInformation(PONG));
    }
}
