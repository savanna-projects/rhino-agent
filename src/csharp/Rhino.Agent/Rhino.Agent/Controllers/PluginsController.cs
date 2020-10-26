/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;
using Rhino.Api.Parser.Contracts;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [Route("api/latest/[controller]")]
    [ApiController]
    public class PluginsController : ControllerBase
    {
        // members: constants
        private const string CountHeader = "Rhino-Total-Specs";

        // members: state
        private readonly RhinoPluginRepository repository;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.PluginsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.PluginsController.</param>
        public PluginsController(IServiceProvider provider)
        {
            repository = provider.GetRequiredService<RhinoPluginRepository>();
        }

        #region *** GET    ***
        // GET: api/v3/plugins
        [HttpGet]
        public Task<IActionResult> Get()
        {
            return DoGet(id: string.Empty);
        }

        // GET api/v3/plugins/<id>
        [HttpGet("{id}")]
        public Task<IActionResult> Get(string id)
        {
            return DoGet(id);
        }

        private async Task<IActionResult> DoGet(string id)
        {
            // setup
            var (statusCode, data) = string.IsNullOrEmpty(id)
                ? repository.Get(Request.GetAuthentication())
                : repository.Get(Request.GetAuthentication(), id);

            // exit conditions
            if (statusCode == HttpStatusCode.NotFound)
            {
                var message = string.IsNullOrEmpty(id) ? "No Plugins found." : $"Plugin [{id}] was not found.";
                return await this.ErrorResultAsync(message, HttpStatusCode.NotFound).ConfigureAwait(false);
            }

            // add count header
            Response.Headers.Add(CountHeader, $"{data.Count()}");

            // response
            return this.ContentTextResult(string.Join(SpecSection.Separator, data), HttpStatusCode.OK);
        }
        #endregion

        #region *** POST   ***
        // POST api/v3/plugins?isPrivate=true
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery(Name = "prvt")] bool isPrivate)
        {
            // setup
            var pluginSpecs = (await Request.ReadAsync().ConfigureAwait(false))
                .Split(SpecSection.Separator)
                .Select(i => i.Trim());

            // create plugins
            var (statusCode, data) = repository.Post(Request.GetAuthentication(), pluginSpecs, isPrivate);

            // response
            if (statusCode == HttpStatusCode.Created)
            {
                var responseData = repository.Get(Request.GetAuthentication()).data.Count();
                Response.Headers[CountHeader] = $"{responseData}";
            }

            var responseBody = new
            {
                Message = "Some plugins were not created.",
                Data = data
            };
            return this.ContentResult(responseBody, HttpStatusCode.Created);
        }
        #endregion

        #region *** DELETE ***
        // DELETE api/v3/plugins?isPrivate=true
        [HttpDelete]
        public IActionResult Delete()
        {
            // delete
            var (statusCode, _) = repository.Delete(Request.GetAuthentication());

            // response
            return this.ContentResult(responseBody: default, statusCode);
        }

        // DELETE api/v3/plugins/:id/?isPrivate=true
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            // delete
            var (statusCode, _) = repository.Delete(Request.GetAuthentication(), id);

            // response
            return this.ContentResult(responseBody: default, statusCode);
        }
        #endregion
    }
}