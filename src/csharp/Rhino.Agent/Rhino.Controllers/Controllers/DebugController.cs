/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet.Engine.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;

using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System.Net.Mime;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        // GET: api/v3/debug
        [HttpPost]
        [SwaggerOperation(
            Summary = "Invoke-Debug",
            Description = "Creates a new _**Debug Session**_.  \n> Note, the API used for these requests is the underline Gravity API.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(OrbitResponse))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<WebAutomation>))]
        public IActionResult Post([SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] WebAutomation automation)
        {
            // results
            var orbitResponse = automation.Send();

            // response
            return Ok(orbitResponse);
        }
    }
}
