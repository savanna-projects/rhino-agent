/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Mvc;

using Rhino.Agent.Extensions;

using System.Net;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [Route("api/latest/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        // GET: api/<PingController>
        [HttpGet]
        public IActionResult Get()
        {
            return this.ContentResult(new { Data = $"{Request.Path} - Pong" }, HttpStatusCode.OK);
        }
    }
}