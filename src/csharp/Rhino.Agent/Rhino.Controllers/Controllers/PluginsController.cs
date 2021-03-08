/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Parser.Contracts;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class PluginsController : ControllerBase
    {
        // members: constants
        private const string CountHeader = "Rhino-Total-Specs";
        private static readonly string doubleLine = Environment.NewLine + Environment.NewLine;
        private static readonly string separator = doubleLine + SpecSection.Separator + doubleLine;

        // members: state
        private readonly IPluginsRepository pluginsRepository;

        // members: private properties
        private Authentication Authentication => Request.GetAuthentication();

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="pluginsRepository">An IPluginsRepository implementation to use with the Controller.</param>
        public PluginsController(IPluginsRepository pluginsRepository)
        {
            this.pluginsRepository = pluginsRepository;
        }

        #region *** Get    ***
        // GET: api/v3/plugins
        [HttpGet]
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get-Plugin -All",
            Description = "Returns a list of available _**Rhino Plugins**_ content.")]
        [Produces(MediaTypeNames.Text.Plain)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(string))]
        public IActionResult Get()
        {
            // get response
            var entities = DoGet(id: string.Empty).Entities;
            Response.Headers[CountHeader] = $"{entities.Count()}";

            // get
            return Ok(string.Join(separator, entities));
        }

        // GET: api/v3/plugins/:id
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get-Plugin -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Returns an existing _**Rhino Plugins**_ content.")]
        [Produces(MediaTypeNames.Text.Plain, MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Get([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get data
            var (statusCode, entity) = DoGet(id);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-plugin -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            Response.Headers[CountHeader] = $"{entity.Count()}";
            return Ok(entity.FirstOrDefault());
        }

        private (int StatusCode, IEnumerable<string> Entities) DoGet(string id)
        {
            // get all
            if (string.IsNullOrEmpty(id))
            {
                var plugins = pluginsRepository.SetAuthentication(Authentication).Get();
                return (StatusCodes.Status200OK, plugins);
            }

            // get one
            var (statusCode, entity) = pluginsRepository.SetAuthentication(Authentication).Get(id);

            // setup
            return (statusCode, new[] { entity });
        }
        #endregion

        #region *** Post   ***
        // POST: api/v3/plugins
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create-Plugin",
            Description = "Creates new or Updates existing one or more _**Rhino Plugin**_.")]
        [Consumes(MediaTypeNames.Text.Plain)]
        [Produces(MediaTypeNames.Text.Plain, MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, SwaggerDocument.StatusCode.Status201Created, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Post([FromQuery(Name = "prvt"), SwaggerParameter(SwaggerDocument.Parameter.Private)] bool isPrivate)
        {
            // setup
            var pluginSpecs = (await Request.ReadAsync().ConfigureAwait(false))
                .Split(SpecSection.Separator)
                .Select(i => i.Trim());

            // create plugins
            pluginsRepository.SetAuthentication(Authentication);
            var plugins = pluginsRepository.Add(pluginSpecs, isPrivate);

            // response
            if (string.IsNullOrEmpty(plugins))
            {
                return Ok();
            }

            // setup            
            var okResponse = pluginsRepository.SetAuthentication(Authentication).Get();
            Response.Headers[CountHeader] = $"{okResponse.Count()}";

            // get
            return Created("/api/v3/plugins", string.Join(separator, okResponse));
        }
        #endregion

        #region *** Delete ***
        // DELETE: api/v3/plugins/:id
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Delete-Plugin -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Deletes an existing _**Rhino Plugin**_.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, SwaggerDocument.StatusCode.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> Delete([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // delete
            var statusCode = pluginsRepository.SetAuthentication(Authentication).Delete(id);

            // results
            return statusCode == StatusCodes.Status404NotFound
                ? await this.ErrorResultAsync<string>($"Delete-Plugin -id {id} = NotFound", statusCode).ConfigureAwait(false)
                : NoContent();
        }

        // DELETE: api/v3/plugins
        [HttpDelete]
        [SwaggerOperation(
            Summary = "Delete-Plugin -All",
            Description = "Deletes all existing _**Rhino Plugin**_.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, SwaggerDocument.StatusCode.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(string))]
        public IActionResult Delete()
        {
            // get credentials
            pluginsRepository.SetAuthentication(Authentication).Delete();

            // results
            return NoContent();
        }
        #endregion
    }
}
