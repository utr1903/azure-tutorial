using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StatsProcessor.Commons;

namespace StatsProcessor.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : Controller
    {
        private readonly ILogger _logger;

        public HealthController(ILogger<HealthController> logger)
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
