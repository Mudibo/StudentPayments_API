using Microsoft.AspNetCore.Mvc;

namespace StudentPayments_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        [Route("/health")]
        public IActionResult Get()
        {
            return Ok("Healthy");
        }
    }
}
