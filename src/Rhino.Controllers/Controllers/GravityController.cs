/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet.Engine.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System.Collections.Concurrent;
using System.Net.Mime;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class GravityController : ControllerBase
    {
        // members
        private static readonly IDictionary<string, object> s_sessions = new ConcurrentDictionary<string, object>();

        // GET: api/v3/gravity/invoke
        [HttpPost, Route("invoke")]
        [SwaggerOperation(
            Summary = "Invoke-OrbitRequest",
            Description = "Creates a new _**Orbit Session**_.  \nNote, the API used for these requests is the underline Gravity API.")]
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

        // POST: api/v3/gravity/debug/start
        [HttpPost, Route("debug/start")]
        public IActionResult DebugStartSession(RhinoConfiguration configuration)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status501NotImplemented
            };
        }

        // POST: api/v3/gravity/debug/forward
        [HttpPost, Route("debug/forward")]
        public IActionResult DebugForward(string session)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status501NotImplemented
            };
        }

        // POST: api/v3/gravity/debug/backward
        [HttpPost, Route("debug/backward")]
        public IActionResult DebugBackward(string session)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status501NotImplemented
            };
        }

        // POST: api/v3/gravity/debug/repeat
        [HttpPost, Route("debug/repeat")]
        public IActionResult DebugRepeat(string session)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status501NotImplemented
            };
        }

        // POST: api/v3/gravity/debug/stop
        [HttpPost, Route("debug/stop")]
        public IActionResult DebugStop(string session)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status501NotImplemented
            };
        }
    }
}
