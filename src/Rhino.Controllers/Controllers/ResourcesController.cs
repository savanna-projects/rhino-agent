/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;

using Swashbuckle.AspNetCore.Annotations;

using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion($"{AppSettings.ApiVersion}.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        // members: state
        private readonly IDomain _domain;
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="domain">An IDomain implementation to use with the Controller.</param>
        public ResourcesController(IDomain domain)
        {
            _domain = domain;
        }

        #region *** Get    ***
        // GET: api/v3/resources
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get-Resource -All",
            Description = "Returns a list of all available _**Resource Files**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ResourceFileModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Get()
        {
            // get response
            var entities = _domain.Resources.Get();
            Response.Headers[RhinoResponseHeader.CountTotalResources] = $"{entities.Count()}";

            // return
            return Ok(entities);
        }

        // GET: api/v3/resources/:id
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get-Resource -Id resource.txt",
            Description = "Returns an existing _**Resource Files**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(ResourceFileModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Get([FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
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

        #region *** Post   ***
        // POST: api/v3/resources
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create-Resource",
            Description = "Creates a new _**Resource File**_.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(ResourceFileModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(GenericErrorModel<ResourceFileModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<ResourceFileModel>))]
        public async Task<IActionResult> Create([FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] ResourceFileModel resourceModel)
        {
            // bad request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // setup
            var (statusCode, entity) = _domain.Resources.Create(entity: resourceModel);

            // bad request
            if (statusCode == StatusCodes.Status400BadRequest)
            {
                return await this
                    .ErrorResultAsync<RhinoConfiguration>("Create-Resource = (BadRequest | NoFileName | NoContent | NoPath)")
                    .ConfigureAwait(false);
            }

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(entity, s_jsonOptions),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = StatusCodes.Status201Created
            };
        }

        // POST: api/v3/resources/bulk
        [HttpPost("bulk")]
        [SwaggerOperation(
            Summary = "Create-Resource -bulk",
            Description = "Creates multiple _**Resource Files**_.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(ResourceFileModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(GenericErrorModel<ResourceFileModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<ResourceFileModel>))]
        public IActionResult Create([FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] IEnumerable< ResourceFileModel> resourceModels)
        {
            // bad request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // setup
            var entities = resourceModels
                .Select(_domain.Resources.Create)
                .Where(i => i.StatusCode == StatusCodes.Status201Created);
            Response.Headers[RhinoResponseHeader.CountTotalResources] = $"{entities.Count()}";

            // get
            return new ContentResult
            {
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = StatusCodes.Status201Created
            };
        }
        #endregion

        #region *** Delete ***
        // DELETE: api/v3/configuration/:id
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Delete-Configuration -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Deletes an existing _**Rhino Configuration**_.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, SwaggerDocument.StatusCode.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Delete([FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get credentials
            var statusCode = _domain.Resources.Delete(id);

            // results
            return statusCode == StatusCodes.Status404NotFound
                ? await this.ErrorResultAsync<string>($"Delete-Resource -Id {id} = NotFound", statusCode).ConfigureAwait(false)
                : NoContent();
        }

        // DELETE: api/v3/resources
        [HttpDelete]
        [SwaggerOperation(
            Summary = "Delete-Resource -All",
            Description = "Deletes all existing _*Resource Files**_.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, SwaggerDocument.StatusCode.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task< IActionResult> Delete()
        {
            // setup
            var statusCode = _domain.Resources.Delete();

            // internal server error
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                return await this
                    .ErrorResultAsync<RhinoConfiguration>("Delete-Resource -All = InternalServerError", statusCode)
                    .ConfigureAwait(false);
            }

            // results
            return NoContent();
        }
        #endregion
    }
}
