/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet.Engine.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Domain;
using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Mime;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion($"{AppSettings.ApiVersion}.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class GravityController : ControllerBase
    {
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
    }
}
