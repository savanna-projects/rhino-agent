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
using Rhino.Api.Contracts.Interfaces;
using Rhino.Api.Parser;
using Rhino.Connectors.Text;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]/[action]")]
    [Route("api/latest/[controller]/[action]")]
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
            client = provider.GetRequiredService<Orbit>();
        }

        // GET api/v3/widget/action
        [HttpGet]
        public IActionResult Actions()
        {
            // get actions
            var actions = manager
                .GetActionsLiteral(Request.GetAuthentication())
                .Where(i => !ExcludeActions.Contains(i.Model.Key) && !string.IsNullOrEmpty(i.Model.Key))
                .OrderBy(i => i.Model.Action.Name);

            // exit conditions
            if (!actions.Any())
            {
                return NotFound();
            }

            // response
            return this.ContentResult(responseBody: actions);
        }

        // GET api/v3/widget/help?action=:action
        [HttpGet]
        public async Task<IActionResult> Help([FromQuery] string action)
        {
            // exit conditions
            if (action.Equals("-1"))
            {
                return NoContent();
            }

            // extract action from manager
            var actions = manager
                .GetActionsLiteral(Request.GetAuthentication())
                .FirstOrDefault(i => i.Model.Action.Name.Equals(action, StringComparison.OrdinalIgnoreCase));

            // response
            if (actions == default)
            {
                return await this
                    .ErrorResultAsync("No actions found", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }
            return this.ContentResult(responseBody: actions);
        }

        // GET api/v3/widget/operators
        [HttpGet]
        public IActionResult Operators()
        {
            // get actions
            var operators = manager.Operators;

            // exit conditions
            if (operators.Count == 0)
            {
                return NotFound();
            }

            // response
            return this.ContentResult(responseBody: operators);
        }

        // POST api/v3/widget/playback
        [HttpPost]
        public async Task<IActionResult> Playback()
        {
            // read test case from request body
            var requestBody = await Request.ReadAsync().ConfigureAwait(false);

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
            return this.ContentResult(responseBody: testResults);
        }

        // GET /api/v3/widget/connectors
        [HttpGet]
        public IActionResult Connectors()
        {
            // setup
            // types loading pipeline
            var onTypes = types.Where(t => typeof(IConnector).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
            var connectorTypes = onTypes.Where(i => i.GetCustomAttribute<ConnectorAttribute>() != null);
            var attributes = connectorTypes.Select(i => i.GetCustomAttribute<ConnectorAttribute>());

            // results
            return this.ContentResult(responseBody: attributes);
        }

        // POST api/v3/widget/send
        [HttpPost]
        public async Task<IActionResult> Send()
        {
            // read test case from request body
            var requestBody = await Request.ReadAsync().ConfigureAwait(false);

            // parse into json token
            var token = JToken.Parse(requestBody);

            // parse test case & configuration
            var configuration = token["config"].ToObject<RhinoConfiguration>();
            var testCaseSrc = JsonConvert.DeserializeObject<string[]>($"{token["test"]}");
            var testSuites = $"{token["suite"]}".Split(";");

            // text connector
            if (configuration.ConnectorConfiguration.Connector.Equals(Connector.Text))
            {
                return this.ContentTextResult(string.Join(Environment.NewLine, testCaseSrc), HttpStatusCode.OK);
            }

            // convert into bridge object
            var testCase = new RhinoTestCaseFactory(client)
                .GetTestCases(string.Join(Environment.NewLine, testCaseSrc.Where(i => !string.IsNullOrEmpty(i))))
                .First();
            testCase.TestSuites = testSuites;
            testCase.Context["comment"] = Api.Extensions.Utilities.GetActionSignature("created");

            // get connector & create test case
            var connectorType = configuration.GetConnector(types);
            if (connectorType == default)
            {
                return NotFound(new { Message = $"Connector [{configuration.ConnectorConfiguration.Connector}] was not found under the connectors repository." });
            }

            var connector = (IConnector)Activator.CreateInstance(connectorType, new object[]
            {
                    configuration, types, logger, false
            });
            connector.ProviderManager.CreateTestCase(testCase);

            // return results
            return this.ContentResult(responseBody: testCase);
        }
    }
}