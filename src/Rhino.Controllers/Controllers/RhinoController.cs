/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System.Net.Mime;
using System.Text.Json;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class RhinoController : ControllerBase
    {
        // members: state
        private readonly IDomain _domain;

        // members: private properties
        private Authentication Authentication => Request.GetAuthentication();

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="domain">An IDomain implementation to use with the Controller.</param>
        public RhinoController(IDomain domain)
        {
            _domain = domain;
        }

        #region *** Configurations ***
        // POST api/v3/rhino/configurations/invoke
        [HttpPost, Route("configurations/invoke")]
        [SwaggerOperation(
            Summary = "Invoke-Configuration",
            Description = "Invokes a single _**Rhino Configuration**_ without saving the configuration under Rhino Server State.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoTestRun))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<RhinoConfiguration>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<RhinoConfiguration>))]
        public IActionResult InvokeConfiguration([FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] RhinoConfiguration configuration)
        {
            // invoke
            var invokeResponse = _domain
                .Rhino
                .SetAuthentication(Authentication)
                .InvokeConfiguration(configuration);

            // get
            return GetInvokeResponse(configuration, invokeResponse);
        }

        // GET api/v3/rhino/configurations/invoke/:id
        [HttpGet, Route("configurations/invoke/{id}")]
        [SwaggerOperation(
            Summary = "Invoke-Configuration -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Invokes a single _**Rhino Configuration**_ without saving the configuration under Rhino Server State.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoTestRun))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult InvokeConfiguration([FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // invoke
            var invokeResponse = _domain.Rhino.SetAuthentication(Authentication).InvokeConfiguration(id);

            // get
            return GetInvokeResponse(id, invokeResponse);
        }
        #endregion

        #region *** Collections    ***
        // POST /rhino/configurations/:id/collections/invoke
        [HttpPost, Route("configurations/{id}/collections/invoke")]
        [SwaggerOperation(
            Summary = "Invoke-Collection -Configuration {00000000-0000-0000-0000-000000000000}",
            Description = "Invokes _**Rhino Spec**_ directly from the request body using preexisting configuration.")]
        [Consumes(MediaTypeNames.Text.Plain)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoTestRun))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> InvokeCollection([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // setup
            var collection = (await Request.ReadAsync().ConfigureAwait(false))
                .Split(RhinoSpecification.Separator)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrEmpty(i));
            var configuration = _domain.Configurations.SetAuthentication(Authentication).Get(id);

            // not found
            if (configuration.StatusCode == StatusCodes.Status404NotFound)
            {
                var notFound = $"Invoke-Collection -Configuration {id} = (NotFound, NoConfiguration)";
                return await this
                    .ErrorResultAsync<string>(notFound, configuration.StatusCode)
                    .ConfigureAwait(false);
            }

            // invoke
            configuration.Entity.TestsRepository = collection;
            var invokeResponse = _domain
                .Rhino
                .SetAuthentication(Authentication)
                .InvokeConfiguration(configuration.Entity);

            // get
            return GetInvokeResponse(collection, invokeResponse);
        }

        // GET /rhino/configurations/:configuration/collections/:collection/invoke
        [HttpGet, Route("configurations/{configuration}/collections/invoke/{collection}")]
        [SwaggerOperation(
            Summary = "Invoke-Collection -Configuration {00000000-0000-0000-0000-000000000000} -Collection {00000000-0000-0000-0000-000000000000}",
            Description = "Invokes _**Rhino Spec**_ from the application state using preexisting configuration.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoTestRun))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> InvokeCollection(
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string configuration,
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string collection)
        {
            // constants
            var error = "Invoke-Collection " +
                    $"-Configuration {configuration} " +
                    $"-Collection {collection} = ($(error), NoCollection | NoConfiguration)";

            // bad request
            var isConfiguration = !string.IsNullOrEmpty(configuration);
            var isCollection = !string.IsNullOrEmpty(collection);

            if (!isConfiguration || !isCollection)
            {
                return await this
                    .ErrorResultAsync<string>(error.Replace("$(error)", "BadRequest"), StatusCodes.Status400BadRequest)
                    .ConfigureAwait(false);
            }

            // setup
            var (collectionStatusCode, collectionEntity) = _domain.Tests.SetAuthentication(Authentication).Get(id: collection);
            var (configurationStatusCode, configurationEntity) = _domain.Configurations.SetAuthentication(Authentication).Get(id: configuration);

            // not found
            isConfiguration = configurationStatusCode == StatusCodes.Status200OK;
            isCollection = collectionStatusCode == StatusCodes.Status200OK;

            if (!isConfiguration || !isCollection)
            {
                return await this
                    .ErrorResultAsync<string>(error.Replace("$(error)", "NotFound"), StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // invoke
            configurationEntity.TestsRepository = collectionEntity.RhinoTestCaseModels.Select(i => i.RhinoSpec);
            var invokeResponse = _domain
                .Rhino
                .SetAuthentication(Authentication)
                .InvokeConfiguration(configurationEntity);

            // get
            return GetInvokeResponse(collection, invokeResponse);
        }

        // GET /rhino/collections/invoke/:id
        [HttpGet, Route("collections/invoke/{id}")]
        [SwaggerOperation(
            Summary = "Invoke-Collection -Configuration All -Collection {00000000-0000-0000-0000-000000000000} -Parallel {True|False}",
            Description = "Invokes _**Rhino Spec**_ from the application state using preexisting collection.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<RhinoTestRun>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> InvokeCollection(
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string id,
            [FromQuery(Name = "parallel"), SwaggerParameter(SwaggerDocument.Parameter.Parallel, Required = false)] bool isParallel,
            [FromQuery(Name = "maxParallel"), SwaggerParameter(SwaggerDocument.Parameter.MaxParallel, Required = false)] int maxParallel = 0)
        {
            // constants
            var error = "Invoke-Collection" +
                " -Configuration All " +
                $"-Collection {id} " +
                $"-Parallel {isParallel} = ($(error), NoCollection | NoConfiguration)";

            // bad request
            if (string.IsNullOrEmpty(id))
            {
                return await this
                    .ErrorResultAsync<string>(error.Replace("$(error)", "BadRequest"), StatusCodes.Status400BadRequest)
                    .ConfigureAwait(false);
            }

            // invoke
            var results = _domain
                .Rhino
                .SetAuthentication(Authentication)
                .InvokeCollection(collection: id, isParallel, maxParallel);

            // not found
            if (results.Count() == 1 && results.First().StatusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>(error.Replace("$(error)", "NotFound"), StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // get
            return Ok(results.Where(i => i.StatusCode == StatusCodes.Status200OK).Select(i => i.TestRun));
        }
        #endregion

        private IActionResult GetInvokeResponse<T>(T entity, (int StatusCode, RhinoTestRun TestRuns) invokeResponse)
        {
            // setup
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // error
            var responseBody = "{}";
            if (invokeResponse.StatusCode > 400)
            {
                var errorResponse = new GenericErrorModel<T>
                {
                    Status = invokeResponse.StatusCode,
                    Request = entity,
                    RouteData = Request.RouteValues
                };
                responseBody = JsonSerializer.Serialize(errorResponse, options);
            }
            else if (invokeResponse.StatusCode < 400)
            {
                responseBody = invokeResponse.StatusCode > 201
                    ? JsonSerializer.Serialize(entity, options)
                    : JsonSerializer.Serialize(invokeResponse.TestRuns, options);
            }

            // get
            return new ContentResult
            {
                Content = responseBody,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = invokeResponse.StatusCode
            };
        }
    }
}
