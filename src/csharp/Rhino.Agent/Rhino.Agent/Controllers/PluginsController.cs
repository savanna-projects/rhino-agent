/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;
using Rhino.Api.Parser.Contracts;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [ApiController]
    public class PluginsController : ControllerBase
    {
        // members: constants
        private readonly string Seperator =
            Environment.NewLine + Environment.NewLine + SpecSection.Separator + Environment.NewLine + Environment.NewLine;
        private const string CountHeader = "Rhino-Total-Specs";

        // members: state
        private readonly RhinoPluginRepository rhinoPlugin;
        private readonly JsonSerializerSettings jsonSettings;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.PluginsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.PluginsController.</param>
        public PluginsController(IServiceProvider provider)
        {
            rhinoPlugin = provider.GetRequiredService<RhinoPluginRepository>();
            jsonSettings = provider.GetRequiredService<JsonSerializerSettings>();
        }

        #region *** GET    ***
        // GET: api/v3/plugins
        [HttpGet]
        public IActionResult Get()
        {
            return DoGet(id: string.Empty);
        }

        // GET api/v3/plugins/<id>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            return DoGet(id);
        }

        private IActionResult DoGet(string id)
        {
            // setup
            var (statusCode, data) = string.IsNullOrEmpty(id)
                ? rhinoPlugin.Get(Request.GetAuthentication())
                : rhinoPlugin.Get(Request.GetAuthentication(), id);

            // exit conditions
            if (statusCode == HttpStatusCode.NotFound)
            {
                var message = string.IsNullOrEmpty(id) ? "No Plugins found." : $"Plugin [{id}] was not found.";
                return NotFound(new { Message = message });
            }

            // add count header
            Response.Headers.Add(CountHeader, $"{data.Count()}");

            // response
            return new ContentResult
            {
                Content = string.Join(Seperator, data),
                ContentType = MediaTypeNames.Text.Plain,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }
        #endregion

        #region *** POST   ***
        // POST api/v3/plugins?isPrivate=true
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery(Name = "prvt")]bool isPrivate)
        {
            // setup
            var pluginSpecs = (await Request.ReadAsync().ConfigureAwait(false)).Split(Seperator);

            // create plugins
            var (statusCode, data) = rhinoPlugin.Post(Request.GetAuthentication(), pluginSpecs, isPrivate);

            // response
            if (statusCode == HttpStatusCode.Created)
            {
                var responseData = rhinoPlugin.Get(Request.GetAuthentication()).data.Count();
                Response.Headers[CountHeader] = $"{responseData}";
                return new ContentResult
                {
                    Content = string.Join(Seperator, responseData),
                    ContentType = MediaTypeNames.Text.Plain,
                    StatusCode = HttpStatusCode.Created.ToInt32()
                };
            }

            var responseBody = new
            {
                Message = "Some plugins were not created.",
                Data = data
            };
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(responseBody, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }
        #endregion

        #region *** DELETE ***
        // DELETE api/v3/plugins?isPrivate=true
        [HttpDelete]
        public IActionResult Delete()
        {
            // delete
            var (statusCode, _) = rhinoPlugin.Delete(Request.GetAuthentication());

            // response
            return new ContentResult { StatusCode = statusCode.ToInt32() };
        }

        // DELETE api/v3/plugins/:id/?isPrivate=true
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            // delete
            var (statusCode, _) = rhinoPlugin.Delete(Request.GetAuthentication(), id);

            // response
            return new ContentResult { StatusCode = statusCode.ToInt32() };
        }
        #endregion
    }
}