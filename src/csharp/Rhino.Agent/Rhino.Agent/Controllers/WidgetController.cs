/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Services.Comet;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;
using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Parser;
using Rhino.Connectors.Text;

using System;
using System.Collections.Generic;
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
        private readonly ILogger logger;
        private readonly IConfiguration appSettings;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Controllers.WidgetController
        /// </summary>
        public WidgetController(IServiceProvider provider)
        {
            appSettings = provider.GetRequiredService<IConfiguration>();
            manager = provider.GetRequiredService<RhinoKbRepository>();
            types = provider.GetRequiredService<IEnumerable<Type>>();
            logger = provider.GetRequiredService<ILogger>().CreateChildLogger(loggerName: nameof(WidgetController));
            jsonSettings = provider.GetRequiredService<JsonSerializerSettings>();
            client = provider.GetRequiredService<Orbit>();
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
            var requestBody = await Request.ReadAsync().ConfigureAwait(false);

            try
            {
                // parse into json token
                var token = JToken.Parse(requestBody);

                // parse test case & configuration
                var configuration = token["config"].ToObject<RhinoConfiguration>();
                configuration.ApplySettings(appSettings);
                configuration.EngineConfiguration.PageLoadTimeout = 60000;
                configuration.EngineConfiguration.ElementSearchingTimeout = 15000;
                configuration.ReportConfiguration.LocalReport = false;
                configuration.ScreenshotsConfiguration.ReturnScreenshots = false;

                // execute case
                var connector = new TextConnector(configuration, types, logger);
                var testResults = connector.Connect().Execute();

                // return results
                return Ok(testResults);
            }
            catch (Exception e) when (e != null)
            {
                logger?.Error(e.Message, e);
                return new ContentResult
                {
                    Content = $"{e}",
                    ContentType = MediaTypeNames.Text.Plain,
                    StatusCode = HttpStatusCode.InternalServerError.ToInt32()
                };
            }
        }

        // GET /api/v3/widget/connectors
        [HttpGet]
        public IActionResult Connectors()
        {
            // setup
            var connectorTypes = types.Where(i => i.GetCustomAttribute<ConnectorAttribute>() != null);
            var attributes = connectorTypes.Select(i => i.GetCustomAttribute<ConnectorAttribute>());

            // results
            var content = JsonConvert.SerializeObject(attributes, jsonSettings);
            return new ContentResult
            {
                Content = content,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // POST api/v3/widget/send
        [HttpPost]
        public async Task<IActionResult> Send()
        {
            // read test case from request body
            var requestBody = await Request.ReadAsync().ConfigureAwait(false);

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
                var connector = configuration.GetConnector(types);
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
    }
}