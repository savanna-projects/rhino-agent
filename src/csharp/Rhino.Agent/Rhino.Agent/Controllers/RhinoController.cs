/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;
using Rhino.Agent.Models;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Extensions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [ApiController]
    public class RhinoController : ControllerBase
    {
        // constants
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

        // members: state
        private readonly IEnumerable<Type> types;
        private readonly RhinoConfigurationRepository configurationRepository;
        private readonly RhinoModelRepository modelRepository;
        private readonly RhinoTestCaseRepository testCaseRepository;
        private readonly JsonSerializerSettings jsonSettings;
        private readonly IConfiguration appSettings;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Controllers.RhinoController.
        /// </summary>
        /// <param name="serviceProvider">Services container.</param>
        public RhinoController(IServiceProvider serviceProvider)
        {
            types = serviceProvider.GetRequiredService<IEnumerable<Type>>();
            configurationRepository = serviceProvider.GetRequiredService<RhinoConfigurationRepository>();
            modelRepository = serviceProvider.GetRequiredService<RhinoModelRepository>();
            testCaseRepository = serviceProvider.GetRequiredService<RhinoTestCaseRepository>();
            jsonSettings = serviceProvider.GetRequiredService<JsonSerializerSettings>();
            appSettings = serviceProvider.GetRequiredService<IConfiguration>();
        }

        #region *** By Configuration  ***
        // GET api/v3/rhino/configuration/<id>
        [HttpGet("configuration/{configuration}")]
        public IActionResult ExecuteByConfiguration(string configuration)
        {
            // get configuration
            var (statusCode, onConfiguration) = GetConfiguration(configuration);

            // failure response
            if (statusCode > HttpStatusCode.OK.ToInt32())
            {
                return new ContentResult { StatusCode = statusCode };
            }

            // process request
            return ExecuteConfigurations(new[] { onConfiguration });
        }

        // POST api/v3/rhino/configuration
        [HttpPost("configuration")]
        public IActionResult ExecuteByConfiguration([FromBody] IEnumerable<string> configurations)
        {
            // get configurations
            var statusCode = HttpStatusCode.OK.ToInt32();

            var onCofigurations = configurations
                .Select(GetConfiguration)
                .Where(i => i.statusCode == statusCode)
                .Select(i => i.configuration);

            // failure response
            if (!onCofigurations.Any())
            {
                return new ContentResult { StatusCode = HttpStatusCode.NotFound.ToInt32() };
            }

            // process request
            return ExecuteConfigurations(onCofigurations);
        }

        // POST api/v3/rhino/execute
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteByConfiguration()
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

            //// get configurations
            var cofiguration = JsonConvert.DeserializeObject<RhinoConfiguration>(requestBody);
            var onCofigurations = new[] { cofiguration };

            // process request
            return ExecuteConfigurations(onCofigurations);
        }
        #endregion

        #region *** By Collection     ***
        // GET api/v3/rhino/collection/<id>
        [HttpGet("collection/{collection}")]
        public IActionResult ExecuteByCollection(string collection)
        {
            return ByCollection(collection, configuration: string.Empty);
        }

        // GET api/v3/rhino/collection/<id>/configuration/<id>
        [HttpGet("collection/{collection}/configuration/{configuration}")]
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
                return new ContentResult { StatusCode = statusCode.ToInt32() };
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
            .Select(GetConfiguration)
            .Where(i => i.statusCode == 200)
            .Select(i => i.configuration);

        private IEnumerable<RhinoConfiguration> Get(RhinoTestCaseCollection collection, string configuration) => collection
            .Configurations
            .Select(GetConfiguration)
            .Where(i => i.statusCode == 200 && $"{i.configuration.Id}".Equals(configuration, Compare))
            .Select(i => i.configuration);
        #endregion

        // execute configurations against Rhino engine
        private IActionResult ExecuteConfigurations(IEnumerable<RhinoConfiguration> configurations)
        {
            // process request
            var testRuns = new ConcurrentBag<RhinoTestRun>();
            foreach (var configuration in configurations)
            {
                ApplyAppSettings(configuration);

                var testRun = configuration.Execute(types);
                testRuns.Add(testRun);
            }

            // process response body
            var content = testRuns.Count == 1
                ? JsonConvert.SerializeObject(testRuns.ElementAt(0), jsonSettings)
                : JsonConvert.SerializeObject(testRuns, jsonSettings);

            // compose response
            return new ContentResult
            {
                Content = content,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        private void ApplyAppSettings(RhinoConfiguration configuration)
        {
            // reporting
            configuration.ReportConfiguration.ReportOut =
                appSettings.GetValue<string>("rhino:reportConfiguration:reportOut");

            configuration.ReportConfiguration.LogsOut =
                appSettings.GetValue<string>("rhino:reportConfiguration:logsOut");

            configuration.ReportConfiguration.Archive =
                appSettings.GetValue<bool>("rhino:reportConfiguration:archive");

            configuration.ReportConfiguration.Reporters = appSettings
                .GetSection("rhino:reportConfiguration:reporters")
                .GetChildren()
                .Select(i => i.Value)
                .ToArray();

            configuration.ReportConfiguration.ConnectionString =
                appSettings.GetValue<string>("rhino:reportConfiguration:connectionString");

            // screenshots
            configuration.ScreenshotsConfiguration.ScreenshotsOut =
                appSettings.GetValue<string>("rhino:screenshotsConfiguration:screenshotsOut");
        }

        #region *** GET Configuration ***
        // get ready to run configuration, by ID
        private (int statusCode, RhinoConfiguration configuration) GetConfiguration(string id)
        {
            // setup
            var credentials = Request.GetAuthentication();
            var (statusCode, configuration) = configurationRepository.Get(credentials, id);

            // not found conditions
            if (statusCode != HttpStatusCode.OK)
            {
                return (HttpStatusCode.NotFound.ToInt32(), default);
            }

            // populate scenarios > populate elements
            configuration.TestsRepository = GetTests(configuration).ToArray();
            configuration.Models = GetModels(configuration).ToArray();

            // bad request conditions
            var isTests = configuration.TestsRepository.Any();
            if (!isTests)
            {
                return (HttpStatusCode.BadRequest.ToInt32(), configuration);
            }

            // results
            return (HttpStatusCode.OK.ToInt32(), configuration);
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