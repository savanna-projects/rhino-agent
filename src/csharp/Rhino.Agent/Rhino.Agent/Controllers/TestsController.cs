/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;
using Rhino.Agent.Models;
using Rhino.Api.Parser.Contracts;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [Route("api/latest/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        // members: constants
        private readonly string Seperator =
            Environment.NewLine + Environment.NewLine + SpecSection.Separator + Environment.NewLine + Environment.NewLine;
        private const string CountHeader = "Rhino-Total-Specs";

        // members: state
        private readonly RhinoTestCaseRepository rhinoTest;
        private readonly RhinoConfigurationRepository rhinoConfiguration;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.TestsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.TestsController.</param>
        public TestsController(IServiceProvider provider)
        {
            rhinoTest = provider.GetRequiredService<RhinoTestCaseRepository>();
            rhinoConfiguration = provider.GetRequiredService<RhinoConfigurationRepository>();
        }

        #region *** GET    ***
        // GET: api/v3/tests
        [HttpGet]
        public IActionResult Get()
        {
            // setup
            var data = rhinoTest.Get(Request.GetAuthentication()).data.Select(i => new
            {
                i.Id,
                i.Configurations,
                Tests = i.RhinoTestCaseDocuments.Count
            });

            // add count header
            Response.Headers.Add(CountHeader, $"{data.Select(i => i.Tests).Sum()}");

            // response
            return this.ContentResult(responseBody: new { Data = new { Collections = data } });
        }

        // GET api/v3/tests/<id>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            // setup
            var obj = rhinoTest.Get(Request.GetAuthentication(), id).data;

            // exit conditions
            if (obj == default)
            {
                return await this
                    .ErrorResultAsync("Collection [{id}] was not found.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }

            // setup
            var specs = obj.RhinoTestCaseDocuments.Select(i => i.RhinoSpec);
            var responseBody = string.Join(Seperator, specs);

            // add count header
            Response.Headers.Add(CountHeader, $"{specs.Count()}");

            // response
            return this.ContentResult(responseBody);
        }

        // GET api/v3/tests/<id>/configuration
        [HttpGet("{id}/configurations")]
        public IActionResult GetConfigurations(string id)
        {
            // setup
            var (_, data) = rhinoTest.Get(Request.GetAuthentication(), id);
            var responseBody = data == default
                ? default
                : new { Data = new { data.Configurations } };

            // response
            return this.ContentResult(responseBody);
        }
        #endregion

        #region *** POST   ***
        // POST api/v3/tests
        [HttpPost]
        public Task<IActionResult> Post()
        {
            return DoPost(configuration: string.Empty);
        }

        // POST api/v3/tests/<configuration>
        [HttpPost("{configuration}")]
        public Task<IActionResult> Post(string configuration)
        {
            return DoPost(configuration);
        }

        private async Task<IActionResult> DoPost(string configuration)
        {
            // read test case from request body
            var requestBody = await Request.ReadAsync().ConfigureAwait(false);
            var documents = requestBody.Split(Seperator);

            // setup
            var collection = new RhinoTestCaseCollection();

            // apply configuration
            var configurationResult = AddConfiguration(onCollection: collection, configuration);

            // validate
            if(configurationResult == HttpStatusCode.BadRequest)
            {
                return await this.ErrorResultAsync("You must provide a configuration").ConfigureAwait(false);
            }
            if(configurationResult == HttpStatusCode.NotFound)
            {
                return await this
                    .ErrorResultAsync($"Configuration [{configuration}] was not found.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }

            // create id for this collection
            collection.Id = Guid.NewGuid();

            // parse test cases
            collection.RhinoTestCaseDocuments = documents
                .Select(i => new RhinoTestCaseDocument { Collection = $"{collection.Id}", Id = Guid.NewGuid(), RhinoSpec = i })
                .Where(i => !string.IsNullOrEmpty(i.RhinoSpec))
                .ToList();

            // get credentials
            var credentials = Request.GetAuthentication();

            // response
            var data = new { Data = new { Id = rhinoTest.Post(credentials, collection) } };
            return this.ContentResult(responseBody: data, HttpStatusCode.Created);
        }

        private HttpStatusCode AddConfiguration(RhinoTestCaseCollection onCollection, string configuration)
        {
            // setup
            onCollection.Configurations ??= new List<string>();

            // no configuration
            if (string.IsNullOrEmpty(configuration))
            {
                return HttpStatusCode.OK;
            }

            // check configuration
            var (statusCode, _) = rhinoConfiguration.Get(Request.GetAuthentication(), configuration);

            // not found
            if (statusCode == HttpStatusCode.NotFound)
            {
                return HttpStatusCode.NotFound;
            }

            // put configuration
            onCollection.Configurations.Add(configuration);
            return HttpStatusCode.OK;
        }
        #endregion

        #region *** PATCH  ***
        // PATCH api/v3/tests/<id>/configurations/<configuration>
        [HttpPatch("{id}/configurations/{configuration}")]
        public async Task<IActionResult> PatchConfiguration(string id, string configuration)
        {
            // patch
            var (statusCode, _) = rhinoTest.Patch(Request.GetAuthentication(), id, configuration);

            // exit conditions
            if (statusCode == HttpStatusCode.BadRequest)
            {
                return await this.ErrorResultAsync("You must provide configuration ID in the request route.").ConfigureAwait(false);
            }
            if (statusCode == HttpStatusCode.NotFound)
            {
                return await this
                    .ErrorResultAsync($"Collection [{id}] or Configuration [{configuration}] were not found.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }

            // response
            return this.ContentResult(responseBody: default, statusCode);
        }

        // PATCH api/v3/tests/<guid>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchTestCases(string id)
        {
            // read test case from request body
            var requestBody = await Request.ReadAsync().ConfigureAwait(false);
            var specs = requestBody.Split(SpecSection.Separator).Select(i => i.Trim());

            // add (generate id)
            var credentials = Request.GetAuthentication();

            // get collection
            var (statusCode, collection) = rhinoTest.Get(credentials, id);
            if (statusCode == HttpStatusCode.NotFound)
            {
                return await this
                    .ErrorResultAsync($"Collection [{id}] was not found.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }
            if (statusCode == HttpStatusCode.BadRequest)
            {
                return await this
                    .ErrorResultAsync("You must provide at least one test case.")
                    .ConfigureAwait(false);
            }

            // create
            var onTestCases = specs.Select(i => new RhinoTestCaseDocument
            {
                Collection = id,
                Id = Guid.NewGuid(),
                RhinoSpec = i
            });

            // apply
            collection.RhinoTestCaseDocuments ??= new List<RhinoTestCaseDocument>();
            foreach (var document in onTestCases)
            {
                collection.RhinoTestCaseDocuments.Add(document);
            }
            rhinoTest.Patch(credentials, collection);

            // response
            return Redirect($"/api/v3/tests/{id}");
        }
        #endregion

        #region *** DELETE ***
        // DELETE api/v3/tests/<guid>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            // execute
            var response = rhinoTest.Delete(Request.GetAuthentication(), id);

            // exit conditions
            if (response == HttpStatusCode.NotFound)
            {
                return await this
                    .ErrorResultAsync($"Collection [{id}] was not found.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }
            return NoContent();
        }

        // DELETE api/v3/tests
        [HttpDelete]
        public IActionResult Delete()
        {
            // execute
            rhinoTest.Delete(Request.GetAuthentication());

            // response
            return NoContent();
        }
        #endregion
    }
}