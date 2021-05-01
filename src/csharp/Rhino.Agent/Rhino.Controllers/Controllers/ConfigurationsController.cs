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

using Swashbuckle.AspNetCore.Annotations;

using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ConfigurationsController : ControllerBase
    {
        // members: state
        private readonly IRepository<RhinoConfiguration> configurationsRepository;

        // members: private properties
        private Authentication Authentication => Request.GetAuthentication();

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="configurationsRepository">An IRepository<RhinoConfiguration> implementation to use with the Controller.</param>
        public ConfigurationsController(IRepository<RhinoConfiguration> configurationsRepository)
        {
            this.configurationsRepository = configurationsRepository;
        }

        #region *** Get    ***
        // GET: api/v3/configuration
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get-Configuration -All",
            Description = "Returns a list of available _**Rhino Configurations**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ConfigurationResponseModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Get()
        {
            // get response
            var configurations = configurationsRepository
                .SetAuthentication(Authentication)
                .Get()
                .Select(GetConfigurationResponse);

            // return
            return Ok(configurations);
        }

        // GET: api/v3/configuration/:id
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get-Configuration -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Returns an existing _**Rhino Configuration**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoConfiguration))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Get([FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get data
            var (statusCode, configuration) = configurationsRepository.SetAuthentication(Authentication).Get(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Configuration -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(configuration);
        }

        private ConfigurationResponseModel GetConfigurationResponse(RhinoConfiguration onConfiguration) => new()
        {
            Id = $"{onConfiguration.Id}",
            Elements = onConfiguration.Models,
            Tests = onConfiguration.TestsRepository
        };
        #endregion

        #region *** Post   ***
        // POST: api/v3/configuration
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create-Configuration",
            Description = "Creates a new _**Rhino Configuration**_.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(RhinoConfiguration))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(GenericErrorModel<RhinoConfiguration>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<RhinoConfiguration>))]
        public async Task<IActionResult> Create([FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] RhinoConfiguration configuration)
        {
            // exit conditions
            if (!configuration.DriverParameters.Any())
            {
                Request.SetBody(configuration);
                return await this
                    .ErrorResultAsync<RhinoConfiguration>("Create-Configuration = (BadRequest, NoDriverParameter)")
                    .ConfigureAwait(false);
            }

            // build
            var id = configurationsRepository.SetAuthentication(Authentication).Add(configuration);
            var responseBody = new { id };

            // get
            return Created($"/api/v3/configuration/{id}", responseBody);
        }
        #endregion

        #region *** Put    ***
        // PUT: api/v3/configuration/:id
        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Update-Configuration -Id {00000000-0000-0000-0000-000000000000} -Configuration {obj}",
            Description  = "Updates an existing _**Rhino Configuration**_.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoConfiguration))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Update(
            [FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string id,
            [FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] RhinoConfiguration configuration)
        {
            // exit conditions
            if (!configuration.DriverParameters.Any())
            {
                return await this
                    .ErrorResultAsync<RhinoConfiguration>("Update-Configuration = (BadRequest, NoDriverParameter)")
                    .ConfigureAwait(false);
            }

            // get results
            var (statusCode, _) = configurationsRepository.SetAuthentication(Authentication).Update(id, entity: configuration);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<RhinoConfiguration>($"Update-Configuration -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // response
            return Redirect($"/api/v3/configurations/{id}");
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
            var statusCode = configurationsRepository.SetAuthentication(Authentication).Delete(id);

            // results
            return statusCode == StatusCodes.Status404NotFound
                ? await this.ErrorResultAsync<string>($"Delete-Configuration -Id {id} = NotFound", statusCode).ConfigureAwait(false)
                : NoContent();
        }

        // DELETE: api/v3/configuration
        [HttpDelete]
        [SwaggerOperation(
            Summary = "Delete-Configuration -All",
            Description = "Deletes all existing _**Rhino Configurations**_ for the authenticated user.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, SwaggerDocument.StatusCode.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Delete()
        {
            // get credentials
            configurationsRepository.SetAuthentication(Authentication).Delete();

            // results
            return NoContent();
        }
        #endregion
    }
}