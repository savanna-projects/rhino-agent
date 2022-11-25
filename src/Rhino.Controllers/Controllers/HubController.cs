/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System.Collections.Generic;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/rhino/[controller]")]
    [ApiController]
    public class HubController : ControllerBase
    {
        // constants
        private readonly static JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // members: injection
        private readonly IDomain _domain;

        public HubController(IDomain domain)
        {
            _domain = domain;
        }

        #region *** Create ***
        // POST: api/v3/rhino/hub/create
        [HttpPost, Route("create")]
        [SwaggerOperation(
            Summary = "Create-Run -Type 'Rhino'",
            Description = "Creates an asynchronous `RhinoTestRun` entity.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, SwaggerDocument.StatusCode.Status201Created)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Create([FromBody] RhinoConfiguration configuration)
        {
            // setup
            var (statusCode, entity) = _domain.Hub.CreateTestRun(configuration);

            // not found
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                var notFound = "Create-Run -Type 'Rhino' = (InternalServerError | NotAbleToCreateRun | Timeout)";
                return await this
                    .ErrorResultAsync<string>(notFound, StatusCodes.Status500InternalServerError)
                    .ConfigureAwait(false);
            }

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(entity, s_jsonOptions),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode
            };
        }
        #endregion

        #region *** Status ***
        // POST: api/v3/rhino/hub/ping
        [HttpGet, Route("ping")]
        [SwaggerOperation(
            Summary = "Get-Ping",
            Description = "Gets an `Http.OK` response is the hub is reachable.")]
        [Produces(MediaTypeNames.Text.Plain)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK)]
        public IActionResult Ping() => Ok("pong");

        // GET: api/v3/rhino/hub/status
        [HttpGet, Route("status")]
        [SwaggerOperation(
            Summary = "Get-Status",
            Description = "Gets the current asynchronous status for both `Rhino` and `Gravity`.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetStatus()
        {
            // setup
            var (statusCode, entity) = _domain.Hub.GetStatus();

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(entity, s_jsonOptions),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode
            };
        }

        // GET: api/v3/rhino/hub/status/:id
        [HttpGet, Route("status/{id}")]
        [SwaggerOperation(
            Summary = "Get-Status -Id '2022.01.01.01.00.00.00.000'",
            Description = "Gets the current asynchronous status for a run by the providing a run id.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetStatus([FromRoute] string id)
        {
            // setup
            var (statusCode, entity) = _domain.Hub.GetStatus(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                var notFound = $"Get-Status -Type 'Rhino' -Id {id} = (NotFound | NoSuchRun | NoTests)";
                return await this
                    .ErrorResultAsync<string>(notFound, StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(entity, s_jsonOptions),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode
            };
        }

        // GET: api/v3/rhino/hub/completed/:id
        [HttpGet, Route("completed/{id}")]
        [SwaggerOperation(
            Summary = "Get-Completed -Id '2022.01.01.01.00.00.00.000'",
            Description = "Gets the a `RhinoTestRun` object from the `Completed` list.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetCompleted([FromRoute] string id)
        {
            // setup
            var (statusCode, entity) = _domain.Hub.GetCompleted(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                var notFound = $"Get-Completed -Type 'Rhino' -Id {id} = (NotFound | NoSuchRun)";
                return await this
                    .ErrorResultAsync<string>(notFound, StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // internal server error
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                var notFound = $"Get-Completed -Type 'Rhino' -Id {id} = InternalServerError";
                return await this
                    .ErrorResultAsync<string>(notFound, StatusCodes.Status500InternalServerError)
                    .ConfigureAwait(false);
            }

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(entity, s_jsonOptions),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode
            };
        }

        // GET: api/v3/rhino/hub/completed
        [HttpGet, Route("completed")]
        [SwaggerOperation(
            Summary = "Get-Completed -All",
            Description = "Gets the collection of `RhinoTestRun` objects from the `Completed` list.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetCompleted()
        {
            // setup
            var (statusCode, entities) = _domain.Hub.GetCompleted();

            // internal server error
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                var notFound = $"Get-Completed -Type 'Rhino' -All = InternalServerError";
                return await this
                    .ErrorResultAsync<string>(notFound, StatusCodes.Status500InternalServerError)
                    .ConfigureAwait(false);
            }

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(entities, s_jsonOptions),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode
            };
        }

        [HttpGet, Route("workers")]
        [SwaggerOperation(
            Summary = "Get-Workers -All",
            Description = "Gets the collection of `WorkerQueueModel` objects from the `Workers` list.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IDictionary<string, WorkerQueueModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetWorkers()
        {
            // setup
            var (statusCode, entities) = _domain.Hub.GetWorkers();

            // internal server error
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                var notFound = $"Get-Workers -Type 'Rhino' -All = InternalServerError";
                return await this
                    .ErrorResultAsync<string>(notFound, StatusCodes.Status500InternalServerError)
                    .ConfigureAwait(false);
            }

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(entities, s_jsonOptions),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode
            };
        }
        #endregion

        #region *** Delete ***
        // Delete: api/v3/rhino/hub/reset
        [HttpDelete, Route("reset")]
        [SwaggerOperation(
            Summary = "Reset-Hub",
            Description = "Removes all the asynchronous runs from the server state.  \n\n> **WARNING!**\n>  \n> This process is irreversible and deletes all the active runs from the server. The results might be unexpected.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status204NoContent, SwaggerDocument.StatusCode.Status204NoContent)]
        public IActionResult Reset()
        {
            // invoke
            _domain.Hub.Reset();

            // get
            return NoContent();
        }
        #endregion
    }
}
