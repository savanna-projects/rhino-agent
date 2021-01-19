/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;

using System;
using System.Net;
using System.Threading.Tasks;

namespace Rhino.Agent.Controllers
{
    [ApiController]
    [Route("api/v3/[controller]")]
    [Route("api/latest/[controller]")]
    public class EnvironmentController : ControllerBase
    {
        // constants
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };
        private readonly JsonSerializerSettings jsonSettingsContent;

        // members: state
        private readonly RhinoEnvironmentRepository repository;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.ConfigurationsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.ConfigurationsController.</param>
        public EnvironmentController(IServiceProvider provider)
        {
            repository = provider.GetRequiredService<RhinoEnvironmentRepository>();
            jsonSettingsContent = provider.GetRequiredService<JsonSerializerSettings>();
        }

        // GET api/v3/environment
        [HttpGet]
        public IActionResult Get()
        {
            // setup
            var process = AutomationEnvironment.SessionParams;
            var server = repository.Get(Request.GetAuthentication()).Model;

            // setup
            return this.ContentResult(
                responseBody: new { process, server },
                statusCode: HttpStatusCode.OK,
                jsonSettings);
        }

        // GET api/v3/environment/<parameterName>
        [HttpGet("{parameterName}")]
        public IActionResult Get([FromRoute] string parameterName)
        {
            // setup conditions
            var isParameter = AutomationEnvironment.SessionParams.ContainsKey(parameterName);

            // setup
            var statusCode = isParameter ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            var responseBody = JsonConvert.DeserializeObject<object>("{}");

            if (isParameter)
            {
                var model = repository.Get(Request.GetAuthentication()).Model;
                var process = AutomationEnvironment.SessionParams[parameterName];
                var server = model.Environment.ContainsKey(parameterName) ? model.Environment[parameterName] : string.Empty;

                responseBody = new { process, server };
            }

            // get
            return this.ContentResult(responseBody, statusCode, jsonSettings);
        }

        // GET api/v3/environment/sync
        [HttpGet, Route("sync")]
        public IActionResult Sync()
        {
            // sync
            var result = repository.Sync(Request.GetAuthentication());

            // result
            if(result == HttpStatusCode.OK)
            {
                return Redirect("/api/v3/environment");
            }

            // error
            var message = result == HttpStatusCode.NotFound
                ? "Was not able to sync parameters into Rhino state. Please check your credentials and make sure the environment exists."
                : "Was not able to sync parameters into Rhino state. The parameters are not available for Rhino.";

            return this.ContentResult(new { Error = message }, result, jsonSettingsContent);
        }

        // GET api/v3/environment/<parameterName>
        [HttpPut("{parameterName}")]
        public async Task<IActionResult> Put([FromRoute] string parameterName)
        {
            // setup
            var value = await Request.ReadAsync().ConfigureAwait(false);

            // put
            var result = repository.Put(Request.GetAuthentication(), parameterName, value);
            AutomationEnvironment.SessionParams[parameterName] = value;

            // result
            if (result == HttpStatusCode.OK)
            {
                return Redirect($"/api/v3/environment/{parameterName}");
            }

            // error
            const string message = "Was not able to sync parameters into Rhino state. The parameters are not available for Rhino.";

            return this.ContentResult(new { Error = message }, result, jsonSettingsContent);
        }

        // GET api/v3/environment/<parameterName>
        [HttpDelete("{parameterName}")]
        public IActionResult Delete([FromRoute] string parameterName)
        {
            // process level state
            if (AutomationEnvironment.SessionParams.ContainsKey(parameterName))
            {
                AutomationEnvironment.SessionParams.Remove(parameterName);
            }

            // delete
            repository.Delete(Request.GetAuthentication(), parameterName);

            // result
            return NoContent();
        }

        // GET api/v3/environment/<parameterName>
        [HttpDelete]
        public IActionResult Delete()
        {
            // delete
            repository.Delete(Request.GetAuthentication());
            AutomationEnvironment.SessionParams.Clear();

            // result
            return NoContent();
        }
    }
}