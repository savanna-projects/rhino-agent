/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet.Engine.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Net.Mime;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Obsolete(message: "This controller is obsolete. Use _**Gravity Controller**_ instead.", error: false)]
    public class DebugController : ControllerBase
    {
        // GET: api/v3/debug
        [HttpPost]
        [SwaggerOperation(
            Summary = "Invoke-Debug",
            Description =
            "**IMPORTANT!**\n" +
            "\n" +
            "This controller is obsolete. Use _**Gravity Controller**_ instead.  \n  \n  \n" +
            "Creates a new _**Debug Session**_.  \nNote, the API used for these requests is the underline Gravity API.")]
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
