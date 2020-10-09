/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.IO;
using System.Threading.Tasks;

using Gravity.Services.Comet.Engine.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Rhino.Agent.Extensions;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [Route("api/latest/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        // GET: api/v3/debug
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

            // get web automation
            var automation = JsonConvert.DeserializeObject<WebAutomation>(requestBody);

            // results
            var orbitResponse = automation.Send();

            // response
            return this.ContentResult(orbitResponse);
        }
    }
}