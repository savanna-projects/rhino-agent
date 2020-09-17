/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Mvc;

using Rhino.Agent.Extensions;

using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "HTTP method cannot be static.")]
        public IActionResult Get()
        {
            return this.ContentTextResult("Pong", HttpStatusCode.OK);
        }
    }
}