using Commons.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InputProcessor.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : Controller
    {
        private readonly ILogger _logger;

        public HealthController(
            ILogger<HealthController> logger
        )
        {
            _logger = logger;
        }

        [HttpGet("")]
        public ActionResult CheckHealth()
        {
            CustomLogger.Log(
                logger: _logger,
                logLevel: LogLevel.Information,
                className: nameof(HealthController),
                methodName: nameof(CheckHealth),
                message: "OK"
            );
            return new OkObjectResult("OK!");
        }
    }
}
