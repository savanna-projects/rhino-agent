/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

using Gravity.Services.Comet.Engine.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Rhino.Agent.Extensions;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        // members: state
        private static JsonSerializerSettings jsonSettings;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.DebugController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.ConfigurationsController.</param>
        public DebugController(IServiceProvider provider)
        {
            jsonSettings = provider.GetRequiredService<JsonSerializerSettings>();
        }

        // GET: api/v3/debug
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {
                // read test case from request body
                using var streamReader = new StreamReader(Request.Body);
                var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                // get web automation
                var automation = JsonConvert.DeserializeObject<WebAutomation>(requestBody);

                // results
                var orbitResponse = automation.Send();

                // response
                var responseBody = JsonConvert.SerializeObject(orbitResponse, jsonSettings);
                return new ContentResult
                {
                    Content = JsonConvert.SerializeObject(responseBody, jsonSettings),
                    ContentType = MediaTypeNames.Application.Json,
                    StatusCode = HttpStatusCode.OK.ToInt32()
                };
            }
            catch (Exception e) when (e != null)
            {
                return new ContentResult
                {
                    Content = $"{e}",
                    ContentType = MediaTypeNames.Text.Plain,
                    StatusCode = HttpStatusCode.InternalServerError.ToInt32()
                };
            }
        }
    }
}