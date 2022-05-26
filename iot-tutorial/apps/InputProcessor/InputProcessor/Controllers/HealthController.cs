using InputProcessor.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InputProcessor.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : Controller
    {
        private ILogger _logger;

        public HealthController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet("")]
        public ActionResult CheckHealth()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(HealthController),
                nameof(CheckHealth),
                "OK"
            );
            return new OkObjectResult("OK!");
        }
    }
}
