/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System.Net.Mime;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        // GET api/v3/ping/rhino
        [HttpGet, Route("rhino")]
        [SwaggerOperation(
            Summary = "Invoke-Ping",
            Description = "Returns _**pong**_ if Rhino server is available.")]
        [Produces(MediaTypeNames.Text.Plain)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        public IActionResult Get() => Ok("pong");
    }
}