/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Rhino.Agent.Extensions;

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

        // GET api/v3/environment
        [HttpGet]
        public IActionResult Get()
        {
            return this.ContentResult(
                responseBody: AutomationEnvironment.SessionParams,
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
            var responseBody = isParameter
                ? AutomationEnvironment.SessionParams[parameterName]
                : JsonConvert.DeserializeObject<object>("{}");

            // get
            return this.ContentResult((object)responseBody, statusCode, jsonSettings);
        }

        // GET api/v3/environment/<parameterName>
        [HttpPut("{parameterName}")]
        public async Task<IActionResult> Put([FromRoute] string parameterName)
        {
            // put
            AutomationEnvironment.SessionParams[parameterName] = await Request.ReadAsync().ConfigureAwait(false);

            // result
            return Redirect($"/api/v3/environment/{parameterName}");
        }

        // GET api/v3/environment/<parameterName>
        [HttpDelete("{parameterName}")]
        public IActionResult Delete([FromRoute] string parameterName)
        {
            // exit conditions
            if (!AutomationEnvironment.SessionParams.ContainsKey(parameterName))
            {
                return NoContent();
            }

            // delete
            AutomationEnvironment.SessionParams.Remove(parameterName);

            // result
            return NoContent();
        }

        // GET api/v3/environment/<parameterName>
        [HttpDelete]
        public IActionResult Delete()
        {
            // delete
            AutomationEnvironment.SessionParams.Clear();

            // result
            return NoContent();
        }
    }
}