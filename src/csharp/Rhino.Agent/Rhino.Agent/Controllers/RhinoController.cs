/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;
using Rhino.Agent.Models;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Engine;
using Rhino.Api.Extensions;
using Rhino.Api.Parser.Contracts;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [Route("api/latest/[controller]")]
    [ApiController]
    public class RhinoController : ControllerBase
    {
        // constants
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;
        private readonly string Seperator =
            Environment.NewLine + Environment.NewLine + SpecSection.Separator + Environment.NewLine + Environment.NewLine;

        // members: state
        private readonly IServiceProvider provider;
        private readonly IEnumerable<Type> types;
        private readonly IConfiguration appSettings;
        private readonly RhinoConfigurationRepository configurationRepository;
        private readonly RhinoModelRepository modelRepository;
        private readonly RhinoTestCaseRepository testCaseRepository;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Controllers.RhinoController.
        /// </summary>
        /// <param name="provider">Services container.</param>
        public RhinoController(IServiceProvider provider)
        {
            // provider
            this.provider = provider;

            // state
            types = provider.GetRequiredService<IEnumerable<Type>>();
            appSettings = provider.GetRequiredService<IConfiguration>();
            configurationRepository = provider.GetRequiredService<RhinoConfigurationRepository>();
            modelRepository = provider.GetRequiredService<RhinoModelRepository>();
            testCaseRepository = provider.GetRequiredService<RhinoTestCaseRepository>();
        }

        #region *** By Configuration  ***
        // GET api/v3/rhino/connect/<id>
        [HttpGet("connect/{configuration}")]
        public IActionResult Connect(string configuration)
        {
            // get configuration
            var (statusCode, onConfiguration) = GetConfiguration(configuration, allowNoTests: false);

            // failure response
            if (statusCode.ToInt32() > HttpStatusCode.OK.ToInt32())
            {
                return this.ContentResult(responseBody: default, statusCode);
            }

            // build
            var automationEngine = new RhinoAutomationEngine(onConfiguration, types);
            var testCases = onConfiguration.Connect(types).ProviderManager.TestRun.TestCases;
            var automations = testCases.SelectMany(i => automationEngine.GetWebAutomation(i));

            // get
            return this.ContentResult(automations);
        }

        // POST api/v3/rhino/connect
        [HttpPost("connect")]
        public IActionResult Connect()
        {
            // get configuration
            var configuration = GetConfiguration();

            // get collection (if any)
            var collections = configuration
                .TestsRepository
                .Select(i => testCaseRepository.Get(Request.GetAuthentication(), i))
                .Where(i => i.data != null);

            var tests = collections.SelectMany(i => i.data.RhinoTestCaseDocuments.Select(i => i.RhinoSpec));

            // apply
            var configurationTests = configuration.TestsRepository.ToList();
            configurationTests.AddRange(tests);
            configuration.TestsRepository = configurationTests;

            // build
            var automationEngine = new RhinoAutomationEngine(configuration, types);
            var testCases = configuration.Connect(types).ProviderManager.TestRun.TestCases;
            var automations = testCases.SelectMany(i => automationEngine.GetWebAutomation(i));

            // get
            return this.ContentResult(automations);
        }

        // GET api/v3/rhino/configurations/<id>
        [HttpGet("configurations/{configuration}")]
        public IActionResult ExecuteByConfiguration(string configuration)
        {
            // get configuration
            var (statusCode, onConfiguration) = GetConfiguration(configuration, allowNoTests: false);

            // failure response
            if (statusCode.ToInt32() > HttpStatusCode.OK.ToInt32())
            {
                return this.ContentResult(responseBody: default, statusCode);
            }

            // process request
            return ExecuteConfigurations(new[] { onConfiguration });
        }

        // POST api/v3/rhino/configurations
        [HttpPost("configurations")]
        public Task<IActionResult> ExecuteByConfiguration()
        {
            return DoExecute();
        }

        // POST api/v3/rhino/execute
        [HttpPost("execute")]
        public IActionResult Execute()
        {
            // get configuration
            var configuration = GetConfiguration();

            // execute
            var responseBody = configuration.Execute(types);

            // response
            return this.ContentResult(responseBody);
        }

        // POST api/v3/rhino/configurations
        [HttpPost("configurations/ids")]
        public Task<IActionResult> ExecuteByConfigurationIds()
        {
            return DoExecute();
        }

        // POST api/v3/rhino/configurations/<id>
        [HttpPost("configurations/{configuration}")]
        public async Task<IActionResult> ExecuteByConfigurationAndSpec(string configuration)
        {
            // get configuration
            var (statusCode, onConfiguration) = GetConfiguration(configuration, allowNoTests: true);

            // failure response
            if (statusCode.ToInt32() > HttpStatusCode.OK.ToInt32())
            {
                return this.ContentResult(responseBody: default, statusCode);
            }

            // build
            var requestBody = await Request.ReadAsync().ConfigureAwait(false);
            onConfiguration.TestsRepository = requestBody.Split(Seperator).Where(i => !string.IsNullOrEmpty(i));

            // process request
            return ExecuteConfigurations(new[] { onConfiguration });
        }

        private async Task<IActionResult> DoExecute()
        {
            // get configurations
            var cofigurations = Request.GetConfigurations(provider);

            // failure response
            if (!cofigurations.Any())
            {
                return await this
                    .ErrorResultAsync("No configurations found or provided.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }

            // process request
            return ExecuteConfigurations(cofigurations);
        }
        #endregion

        #region *** By Collection     ***
        // GET api/v3/rhino/collections/<id>
        [HttpGet("collections/{collection}")]
        public IActionResult ExecuteByCollection(string collection)
        {
            return ByCollection(collection, configuration: string.Empty);
        }

        // GET api/v3/rhino/collections/<id>/configurations/<id>
        [HttpGet("collections/{collection}/configurations/{configuration}")]
        public IActionResult ExecuteByCollection(string collection, string configuration)
        {
            return ByCollection(collection, configuration);
        }

        private IActionResult ByCollection(string collection, string configuration)
        {
            // setup
            var credentials = Request.GetAuthentication();

            // get test collection
            var (statusCode, onCollection) = testCaseRepository.Get(credentials, id: collection);
            if (statusCode != HttpStatusCode.OK)
            {
                return this.ContentResult(responseBody: default, statusCode);
            }

            // setup configurations > setup scenarios
            var onCofigurations = string.IsNullOrEmpty(configuration)
                ? Get(onCollection)
                : Get(onCollection, configuration);

            var onScenarios = onCollection
                .RhinoTestCaseDocuments
                .SelectMany(i => i.RhinoSpec.Split(">>>").Select(j => j.Trim()));

            // override configuration
            foreach (var onConfiguration in onCofigurations)
            {
                onConfiguration.TestsRepository = onScenarios.ToArray();
            }

            // process request
            return ExecuteConfigurations(onCofigurations);
        }

        private IEnumerable<RhinoConfiguration> Get(RhinoTestCaseCollection collection) => collection
            .Configurations
            .Select(i=> GetConfiguration(i, allowNoTests: false))
            .Where(i => i.statusCode == HttpStatusCode.OK)
            .Select(i => i.configuration);

        private IEnumerable<RhinoConfiguration> Get(RhinoTestCaseCollection collection, string configuration) => collection
            .Configurations
            .Select(i=> GetConfiguration(i, allowNoTests: false))
            .Where(i => i.statusCode == HttpStatusCode.OK && $"{i.configuration.Id}".Equals(configuration, Compare))
            .Select(i => i.configuration);
        #endregion

        // execute configurations against Rhino engine
        private IActionResult ExecuteConfigurations(IEnumerable<RhinoConfiguration> configurations)
        {
            // process request
            var testRuns = new ConcurrentBag<RhinoTestRun>();
            foreach (var configuration in configurations)
            {
                var onConfiguration = configuration.ApplySettings(appSettings);

                var testRun = onConfiguration.Execute(types);
                testRuns.Add(testRun);
            }

            // process response body
            var content = testRuns.Count == 1 ? (object)testRuns.ElementAt(0) : testRuns;

            // compose response
            return this.ContentResult(responseBody: content);
        }

        #region *** GET Configuration ***
        // TODO: implement recursive conversion JToken to <string, object>
        // get ready to run configuration, by request body
        private RhinoConfiguration GetConfiguration()
        {
            // get configuration
            var requestBody = Request.ReadAsync().GetAwaiter().GetResult();
            var configuration = JsonConvert.DeserializeObject<RhinoConfiguration>(requestBody).ApplySettings(appSettings);

            // get provider capabilities
            var jsonObject = JObject.Parse(requestBody);
            var capabilities = jsonObject.SelectToken("capabilities");
            var options = jsonObject.SelectToken($"capabilities.{configuration.ConnectorConfiguration.Connector}:options");

            var onOptions = options != null
                ? JsonConvert.DeserializeObject<IDictionary<string, object>>($"{options}")
                : new Dictionary<string, object>();

            configuration.Capabilities = capabilities != null
                ? JsonConvert.DeserializeObject<IDictionary<string, object>>($"{capabilities}")
                : new Dictionary<string, object>();

            configuration.Capabilities[$"{configuration.ConnectorConfiguration.Connector}:options"] = onOptions;

            // get
            return configuration;
        }

        // get ready to run configuration, by ID
        private (HttpStatusCode statusCode, RhinoConfiguration configuration) GetConfiguration(string id, bool allowNoTests)
        {
            // setup
            var credentials = Request.GetAuthentication();
            var (statusCode, configuration) = configurationRepository.Get(credentials, id);

            // not found conditions
            if (statusCode != HttpStatusCode.OK)
            {
                return (HttpStatusCode.NotFound, default);
            }

            // populate scenarios > populate elements
            configuration.TestsRepository = GetTests(configuration).ToArray();
            configuration.Models = GetModels(configuration).ToArray();

            // bad request conditions
            var isTests = configuration.TestsRepository.Any();
            if (!isTests && !allowNoTests)
            {
                return (HttpStatusCode.BadRequest, configuration);
            }

            // results
            return (HttpStatusCode.OK, configuration);
        }

        // collect scenarios for configuration
        private IEnumerable<string> GetTests(RhinoConfiguration configuration)
        {
            // setup
            var credentials = Request.GetAuthentication();
            var scenarios = new List<string>();

            // collection
            foreach (var id in configuration.TestsRepository)
            {
                if (!Regex.IsMatch(input: id, pattern: @"\w{8}-(\w{4}-){3}\w{12}"))
                {
                    continue;
                }
                var (statusCode, scenariosCollection) = testCaseRepository.Get(credentials, id);
                if (statusCode == HttpStatusCode.NotFound)
                {
                    continue;
                }
                var onScenarios = scenariosCollection
                    .RhinoTestCaseDocuments
                    .SelectMany(i => i.RhinoSpec.Split(">>>").Select(j => j.Trim()));
                scenarios.AddRange(onScenarios);
            }

            // results
            return scenarios;
        }

        // collect elements for configuration
        private IEnumerable<string> GetModels(RhinoConfiguration configuration)
        {
            // setup
            var credentials = Request.GetAuthentication();
            var elements = new List<string>();

            // collection
            foreach (var id in configuration.Models)
            {
                if (!Regex.IsMatch(input: id, pattern: @"\w{8}-(\w{4}-){3}\w{12}"))
                {
                    continue;
                }
                var (statusCode, modelsCollection) = modelRepository.Get(credentials, id);
                if (statusCode == HttpStatusCode.NotFound)
                {
                    continue;
                }
                elements.Add(JsonConvert.SerializeObject(modelsCollection.Models));
            }

            // results
            return elements;
        }
        #endregion
    }
}