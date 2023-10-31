/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Controllers.Domain;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;
using Rhino.Settings;

using Swashbuckle.AspNetCore.Annotations;

using System.Net.Mime;
using System.Text.Json;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion($"{AppSettings.ApiVersion}.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class GravityController : ControllerBase
    {
        // members: static
        private static readonly JsonSerializerOptions s_options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // members: state
        private readonly IGravityRepository _domain;

        public GravityController(IGravityRepository domain)
        {
            _domain = domain;
        }

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
            // invoke
            var (statusCode, response) = _domain.Invoke(automation);

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(response, s_options),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode
            };
        }

        // POST: api/v3/gravity/convert
        [HttpPost, Route("convert")]
        [SwaggerOperation(
            Summary = "ConvertTo-ActionRule",
            Description = "Converts _**Rhino Step**_ to an _**Action Rule**_ object.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(ActionRule))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Convert([FromBody] ActionRuleConvertModel model)
        {
            // bad request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // invoke
            var (statusCode, actionRule) = _domain.Convert(model.Action);

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(actionRule, s_options),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode
            };
        }
    }
}
