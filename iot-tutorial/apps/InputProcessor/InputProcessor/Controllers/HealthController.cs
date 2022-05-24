using Microsoft.AspNetCore.Mvc;

namespace InputProcessor.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : Controller
    {
        [HttpGet("")]
        public ActionResult CheckHealth()
        {
            return new OkObjectResult("OK!");
        }
    }
}
