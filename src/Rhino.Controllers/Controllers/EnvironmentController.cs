/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;

using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System.Net.Mime;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        // members: state
        private readonly IDomain _domain;

        // members: private properties
        private Authentication Authentication => Request.GetAuthentication();

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="domain">An IDomain implementation to use with the Controller.</param>
        public EnvironmentController(IDomain domain)
        {
            _domain = domain;
        }

        #region *** Get    ***
        // GET: api/v3/environment
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get-EnvironmentParameter -All",
            Description = "Returns a list of available _**Rhino Parameters**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IDictionary<string, object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Get()
        {
            // get response
            var parameters = _domain.Environments.SetAuthentication(Authentication).Get();

            // return
            return Ok(new Dictionary<string, object>(parameters));
        }

        // GET: api/v3/environment/:name
        [HttpGet("{name}")]
        [SwaggerOperation(
            Summary = "Get-EnvironmentParameters -Name {parameterKey}",
            Description = "Returns the value of the specified _**Rhino Parameter**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IDictionary<string, object>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Get([FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string name)
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

        // GET: api/v3/environment/sync
        [HttpGet, Route("sync")]
        [SwaggerOperation(
            Summary = "Sync-EnvironmentParameter",
            Description = "Sync environment parameters with _**Rhino State Parameters**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IDictionary<string, object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Sync()
        {
            // setup
            _domain.Environments.SetAuthentication(Authentication);
            var entities = _domain.Environments.Sync().Entities;

            // get
            return Ok(entities);
        }
        #endregion

        #region *** Post   ***
        // POST: api/v3/environment
        [HttpPost]
        [SwaggerOperation(
            Summary = "Add-EnvironmentParameters -Name {parameterKey}",
            Description = "Updates a set of _**Rhino Parameter**_ if the parameters exists or create a new one if not.")]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, SwaggerDocument.StatusCode.Status201Created, Type = typeof(IDictionary<string, object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<IDictionary<string, object>>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<IDictionary<string, object>>))]
        public async Task<IActionResult> Add(
            [FromBody, SwaggerParameter(SwaggerDocument.Parameter.Entity)] IDictionary<string, object> value)
        {
            // bad request
            if (value == null || !value.Any())
            {
                return await this
                    .ErrorResultAsync<IDictionary<string, object>>($"Add-EnvironmentParameters = (BadRequest, NoValue | NoKey)")
                    .ConfigureAwait(false);
            }

            // build
            _domain.Environments.SetAuthentication(Authentication);
            _domain.Environments.Add(value);

            // build
            var responseBody = new Dictionary<string, object>(value, StringComparer.OrdinalIgnoreCase);

            // get
            return Created($"/api/v3/environment", responseBody);
        }
        #endregion

        #region *** Put    ***
        // PUT: api/v3/environment
        [HttpPut("{name}")]
        [SwaggerOperation(
            Summary = "Update-EnvironmentParameter -Name {parameterKey}",
            Description = "Updates the value of the specified _**Rhino Parameter**_ if the parameter exists or create a new one if not.")]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Text.Plain)]
        [SwaggerResponse(StatusCodes.Status201Created, SwaggerDocument.StatusCode.Status201Created, Type = typeof(IDictionary<string, object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<IDictionary<string, object>>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<IDictionary<string, object>>))]
        public async Task<IActionResult> Update(
            [FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string name,
            [FromBody, SwaggerParameter(SwaggerDocument.Parameter.Entity)] string value)
        {
            // bad request
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(name))
            {
                Request.SetBody(new Dictionary<string, object>
                {
                    [string.IsNullOrEmpty(name) ? "" : name] = value
                });
                return await this
                    .ErrorResultAsync<IDictionary<string, object>>($"Update-EnvironmentParameter -Key {name} = (BadRequest, NoValue | NoKey)")
                    .ConfigureAwait(false);
            }

            // build
            _domain
                .Environments
                .SetAuthentication(Authentication)
                .Add(new KeyValuePair<string, object>(name, value));

            var responseBody = new Dictionary<string, object>
            {
                [name] = value
            };

            // get
            return Created($"/api/v3/environment/{name}", responseBody);
        }
        #endregion

        #region *** Delete ***
        // DELETE: api/v3/environment/:name
        [HttpDelete("{name}")]
        [SwaggerOperation(
            Summary = "Delete-EnvironmentParameter -Name {parameterKey}",
            Description = "Deletes _**Rhino Parameter**_ if the parameter exists.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Delete([FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string name)
        {
            // get credentials
            _domain.Environments.SetAuthentication(Authentication).Delete(name);
            var statusCode = _domain.Environments.DeleteByName(name);

            // results
            return statusCode == StatusCodes.Status404NotFound
                ? await this.ErrorResultAsync<string>($"Delete-EnvironmentParameter -Name {name} = NotFound").ConfigureAwait(false)
                : NoContent();
        }

        // DELETE: api/v3/environment
        [HttpDelete]
        [SwaggerOperation(
            Summary = "Delete-EnvironmentParameter -All",
            Description = "Deletes _**Rhino Parameter**_ if the parameter exists.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Delete()
        {
            // delete
            _domain.Environments.SetAuthentication(Authentication).Delete();

            // get
            return NoContent();
        }
        #endregion
    }
}
