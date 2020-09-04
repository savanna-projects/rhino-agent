/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet;
using Gravity.Services.Comet.Engine.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;
using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Contracts.Interfaces;
using Rhino.Api.Parser;
using Rhino.Connectors.Text;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]/[action]")]
    [ApiController]
    public class WidgetController : ControllerBase
    {
        // constants
        private static readonly string[] ExcludeActions = new[]
        {
            ActionType.Assert,
            ActionType.BannersListener,
            ActionType.Condition,
            ActionType.ExtractData,
            ActionType.Repeat
        };

        // members
        private readonly JsonSerializerSettings jsonSettings;
        private readonly RhinoKbRepository manager;
        private readonly Orbit client;
        private readonly IEnumerable<Type> types;
        private readonly ILogger<WidgetController> logger;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Controllers.WidgetController
        /// </summary>
        public WidgetController(IServiceProvider serviceProvider)
        {
            manager = serviceProvider.GetRequiredService<RhinoKbRepository>();
            types = serviceProvider.GetRequiredService<IEnumerable<Type>>();
            logger = serviceProvider.GetRequiredService<ILogger<WidgetController>>();
            jsonSettings = serviceProvider.GetRequiredService<JsonSerializerSettings>();
            client = serviceProvider.GetRequiredService<Orbit>();
        }

        // GET api/v3/widget/action
        [HttpGet]
        public IActionResult Actions()
        {
            // get actions
            var actions = manager
                .GetActionsLiteral(Request.GetAuthentication())
                .Where(i => !ExcludeActions.Contains(i.Key) && !string.IsNullOrEmpty(i.Key))
                .OrderBy(i => i.Action.Name);

            // serialize results
            var value = JsonConvert.SerializeObject(actions, jsonSettings);

            // exit conditions
            if (!actions.Any())
            {
                return NotFound();
            }

            // response
            return new ContentResult
            {
                Content = value,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET api/v3/widget/help?action=:action
        [HttpGet]
        public IActionResult Help([FromQuery] string action)
        {
            // exit conditions
            if (action.Equals("-1"))
            {
                return NoContent();
            }

            // extract action from manager
            var actions = manager
                .GetActionsLiteral(Request.GetAuthentication())
                .FirstOrDefault(i => i.Action.Name.Equals(action, StringComparison.OrdinalIgnoreCase));
            var value = JsonConvert.SerializeObject(actions, jsonSettings);

            // exit conditions
            return actions == default ? (IActionResult)NotFound() : Ok(value);
        }

        // GET api/v3/widget/operators
        [HttpGet]
        public IActionResult Operators()
            => manager.Operators.Count == 0 ? (IActionResult)NotFound() : Ok(manager.Operators);

        // POST api/v3/widget/playback
        [HttpPost]
        public async Task<IActionResult> Playback()
        {
            // read test case from request body
            var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

            try
            {
                // parse into json token
                var token = JToken.Parse(requestBody);

                // parse test case & configuration
                var configuration = token["config"].ToObject<RhinoConfiguration>();
                configuration.EngineConfiguration.PageLoadTimeout = 60000;
                configuration.EngineConfiguration.ElementSearchingTimeout = 15000;

                // execute case
                var connector = new TextConnector(configuration, types);
                var testResults = connector.Connect().Execute();

                // return results
                return Ok(testResults);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, ex.Message, requestBody);
                return new StatusCodeResult(500);
            }
            finally
            {
                streamReader.Dispose();
            }
        }

        // POST api/v3/widget/send
        [HttpPost]
        public async Task<IActionResult> Send()
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

            try
            {
                // parse into json token
                var token = JToken.Parse(requestBody);

                // parse test case & configuration
                var configuration = token["config"].ToObject<RhinoConfiguration>();
                var testCaseSrc = JsonConvert.DeserializeObject<string[]>($"{token["test"]}");
                var testSuite = $"{token["suite"]}";

                // convert into bridge object
                var testCase = new RhinoTestCaseFactory(client).GetTestCases(string.Join(Environment.NewLine, testCaseSrc.Where(i => !string.IsNullOrEmpty(i)))).First();
                testCase.TestSuite = testSuite;
                testCase.Context["comment"] = $"{{noformat}}{DateTime.Now:yyyy-MM-dd hh:mm:ss}: Created by Rhino widget{{noformat}}";

                // get connector & create test case
                var connector = SetConnector(types, configuration);
                if(connector == default)
                {
                    return NotFound(new { Message = $"Connector [{configuration.Connector}] was not found under the connectors repository." });
                }

                connector.ProviderManager.CreateTestCase(testCase);

                // return results
                return new ContentResult
                {
                    Content = JsonConvert.SerializeObject(testCase),
                    ContentType = MediaTypeNames.Application.Json,
                    StatusCode = HttpStatusCode.Created.ToInt32()
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

        // POST api/v3/widget/run
        [HttpPost]
        public async Task<IActionResult> Run()
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

            try
            {
                // parse test case & configuration
                var configuration = JsonConvert.DeserializeObject<RhinoConfiguration>(requestBody);

                // get connector & create test case
                var connector = SetConnector(types, configuration);
                var testResults = connector.Connect().Execute();

                // return results
                var statusCode = testResults.TestCases.Any(i => !i.Actual)
                    ? HttpStatusCode.BadRequest
                    : HttpStatusCode.OK;

                // build failed results dictionary
                var failedResults = new Dictionary<string, string>();
                foreach (var failedResult in testResults.TestCases.Where(i => !i.Actual))
                {
                    failedResults[failedResult.Key] = failedResult.Link;
                }
                return statusCode == HttpStatusCode.BadRequest ? BadRequest(failedResults) : (IActionResult)Ok();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, ex.Message, requestBody);
                return new StatusCodeResult(500);
            }
        }

        // POST api/v3/widget/automation
        [HttpPost]
        public async Task<IActionResult> Automation()
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

            try
            {
                // parse test case & configuration
                var automation = JsonConvert.DeserializeObject<WebAutomation>(requestBody);

                // return results
                var response = automation.Send();
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, ex.Message, requestBody);
                return new StatusCodeResult(500);
            }
        }

        private IConnector SetConnector(IEnumerable<Type> types, RhinoConfiguration configuration)
        {
            // constants
            const StringComparison C = StringComparison.OrdinalIgnoreCase;

            // types loading pipeline
            var byContract = types.Where(t => typeof(IConnector).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
            var byAttribute = byContract.Where(t => t.GetCustomAttribute<ConnectorAttribute>() != null);

            // get connector type by it's name
            var type = byAttribute
                .FirstOrDefault(t => t.GetCustomAttribute<ConnectorAttribute>().Name.Equals(configuration.Connector, C));

            if (type == default)
            {
                return default;
            }

            // activate new connector instance
            return (IConnector)Activator.CreateInstance(type, new object[] { configuration, types });
        }
    }
}