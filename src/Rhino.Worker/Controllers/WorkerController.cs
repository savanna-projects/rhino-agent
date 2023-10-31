/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;

using Swashbuckle.AspNetCore.Annotations;

using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Linq;

namespace Rhino.Worker.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        // members
        private readonly IDomain _domain;
        private readonly string _logPath;

        // members: private properties
        private Authentication Authentication => Request.GetAuthentication();

        /// <summary>
        /// Initialize a new instance of WorkerController object.
        /// </summary>
        /// <param name="domain">The IDomain implementation to use with the controller.</param>
        public WorkerController(IDomain domain)
        {
            // setup
            _domain = domain;

            // get in-folder
            var inFolder = domain.AppSettings.ReportsAndLogs.LogsOut;
            _logPath = string.IsNullOrEmpty(inFolder) ? ControllerUtilities.LogsDefaultFolder : inFolder;
        }

        // GET api/v3/worker/ping
        [HttpGet, Route("ping")]
        [SwaggerOperation(
            Summary = "Invoke-Ping",
            Description = "Returns _**pong**_ if Worker service is available.")]
        [Produces(MediaTypeNames.Text.Plain)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        public IActionResult Ping() => Ok("Pong");

        #region *** Plugins     ***
        // GET: api/v3/worker/plugins
        [HttpGet, Route("plugins")]
        [SwaggerOperation(
            Summary = "Get-Plugin -All",
            Description = "Returns a list of available _**Rhino Plugins**_ content.")]
        [Produces(MediaTypeNames.Text.Plain)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(string))]
        public IActionResult GetPlugins()
        {
            // get response
            var entities = InvokeGetPlugins(id: string.Empty).Entities;
            Response.Headers[RhinoResponseHeader.CountTotalSpecs] = $"{entities.Count()}";

            // get
            return Ok(string.Join(Utilities.Separator, entities));
        }

        // GET: api/v3/worker/plugins/:id
        [HttpGet, Route("plugins/{id}")]
        [SwaggerOperation(
            Summary = "Get-Plugin -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Returns an existing _**Rhino Plugins**_ content.")]
        [Produces(MediaTypeNames.Text.Plain, MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetPlugins([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get data
            var (statusCode, entity) = InvokeGetPlugins(id);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-plugin -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            Response.Headers[RhinoResponseHeader.CountTotalSpecs] = $"{entity.Count()}";
            return Ok(entity.FirstOrDefault());
        }

        private (int StatusCode, IEnumerable<string> Entities) InvokeGetPlugins(string id)
        {
            // get all
            if (string.IsNullOrEmpty(id))
            {
                var plugins = _domain.Plugins.SetAuthentication(Authentication).Get();
                return (StatusCodes.Status200OK, plugins);
            }

            // get one
            var (statusCode, entity) = _domain.Plugins.SetAuthentication(Authentication).Get(id);

            // setup
            return (statusCode, new[] { entity });
        }
        #endregion

        #region *** Models      ***
        // GET: api/v3/worker/models
        [HttpGet, Route("models")]
        [SwaggerOperation(
            Summary = "Get-RhinoModelCollection -All",
            Description = "Returns a list of available _**Rhino Models**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ModelCollectionResponseModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetModels()
        {
            // local 
            static ModelCollectionResponseModel GetCollection(RhinoModelCollection collection) => new()
            {
                Id = $"{collection.Id}",
                Configurations = collection.Configurations,
                Models = collection.Models.Count,
                Entries = collection.Models.SelectMany(i => i.Entries).Count()
            };

            // setup
            var responseBody = _domain
                .Models
                .SetAuthentication(Authentication)
                .Get()
                .Select(GetCollection);

            // response
            return Ok(responseBody);
        }

        // GET api/v3/worker/models/:id
        [HttpGet, Route("models/{id}")]
        [SwaggerOperation(
            Summary = "Get-RhinoModelCollection -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Returns an existing _**Rhino Model**_ collection.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoModelCollection))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetModels([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get data
            var (statusCode, modelCollection) = _domain.Models.SetAuthentication(Authentication).Get(id);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-RhinoModelCollection -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(modelCollection);
        }
        #endregion

        #region *** Environment ***
        // GET: api/v3/worker/environment
        [HttpGet, Route("environment")]
        [SwaggerOperation(
            Summary = "Get-EnvironmentParameter -All",
            Description = "Returns a list of available _**Rhino Parameters**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IDictionary<string, object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetEnvironment()
        {
            // get response
            var parameters = _domain.Environments.SetAuthentication(Authentication).Get();

            // return
            return Ok(new Dictionary<string, object>(parameters));
        }

        // GET: api/v3/worker/environment/:name
        [HttpGet, Route("environment/{name}")]
        [SwaggerOperation(
            Summary = "Get-EnvironmentParameters -Name {parameterKey}",
            Description = "Returns the value of the specified _**Rhino Parameter**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IDictionary<string, object>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetEnvironment([FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string name)
        {
            // get response
            _domain.Environments.SetAuthentication(Authentication);
            var (statusCode, entity) = _domain.Environments.GetByName(name);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-EnvironmentParameter -Name {name} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(new Dictionary<string, object>
            {
                [entity.Key] = entity.Value
            });
        }
        #endregion

        #region *** Resources   ***
        // GET: api/v3/worker/resources
        [HttpGet, Route("Resources")]
        [SwaggerOperation(
            Summary = "Get-Resource -All",
            Description = "Returns a list of all available _**Resource Files**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ResourceFileModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetResources()
        {
            // get response
            var entities = _domain.Resources.Get();
            Response.Headers[RhinoResponseHeader.CountTotalResources] = $"{entities.Count()}";

            // return
            return Ok(entities);
        }

        // GET: api/v3/worker/resources/:id
        [HttpGet, Route("resources/{id}")]
        [SwaggerOperation(
            Summary = "Get-Resource -Id resource.txt",
            Description = "Returns an existing _**Resource Files**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(ResourceFileModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetResources([FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get data
            var (statusCode, entity) = _domain.Resources.Get(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Resource -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }
        #endregion

        #region *** Logs        ***
        // GET: api/v3/worker/logs
        [HttpGet, Route("logs")]
        [SwaggerOperation(
            Summary = "Get-Log -All",
            Description = "Returns an existing _**Automation Logs**_ files list.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetLogs()
        {
            // get
            var responseBody = _domain.Logs.Get(_logPath);

            // response
            return Ok(responseBody);
        }

        // GET: api/v3/worker/logs/:id
        [HttpGet, Route("logs/{id}")]
        [SwaggerOperation(
            Summary = "Get-Log -Id {yyyy-MM-dd.log}",
            Description = "Returns an existing _**Automation Log**_.")]
        [Produces(MediaTypeNames.Text.Plain, MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetLogs([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get
            var (statusCode, responseBody) = await _domain.Logs.GetAsync(_logPath, id).ConfigureAwait(false);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Log -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // response
            return Ok(responseBody);
        }
        #endregion
    }
}
