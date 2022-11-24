using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Rhino.Worker.Controllers
{
    [Route("api/v3/rhino/[controller]")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        [HttpGet, Route("ping")]
        public IActionResult Ping() => Ok("Pong");
    }
}
