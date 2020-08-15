/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;
using Rhino.Agent.Models;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        // members: constants
        private readonly string Seperator =
            Environment.NewLine + Environment.NewLine + ">>>" + Environment.NewLine + Environment.NewLine;
        private readonly string Splitter = ">>>";

        // members: state
        private readonly RhinoTestCaseRepository repository;
        private readonly JsonSerializerSettings jsonSettings;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.TestsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.ModelsController.</param>
        public TestsController(IServiceProvider provider)
        {
            repository = provider.GetRequiredService<RhinoTestCaseRepository>();
            jsonSettings = provider.GetRequiredService<JsonSerializerSettings>();
        }

        // GET: api/v3/collection
        [HttpGet]
        public IActionResult Get()
        {
            // setup
            var data = repository.Get(Request.GetAuthentication()).data.Select(i => new
            {
                i.Id,
                i.Configurations,
                TotalScenarios = i.RhinoTestCaseDocuments.Count
            });

            // response
            var responseBody = JsonConvert.SerializeObject(new { Data = new { Collections = data } }, jsonSettings);
            return new ContentResult
            {
                Content = responseBody,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET api/v3/collection/<id>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            // setup
            var data = repository.Get(Request.GetAuthentication(), id).data;
            var body = string.Join(Seperator, data.RhinoTestCaseDocuments.Select(i => i.RhinoSpec));

            // response
            return new ContentResult
            {
                Content = body,
                ContentType = MediaTypeNames.Text.Plain,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET api/v3/collection/<id>/configuration
        [HttpGet("{id}/configurations")]
        public IActionResult GetConfigurations(string id)
        {
            // setup
            var (statusCode, data) = repository.Get(Request.GetAuthentication(), id);
            var content = data == default
                ? string.Empty
                : JsonConvert.SerializeObject(new { Data = new { data.Configurations } }, jsonSettings);

            // response
            return new ContentResult
            {
                Content = content,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode.ToInt32()
            };
        }

        // POST api/v3/collection/<configuration>
        [HttpPost("{configuration}")]
        public async Task<IActionResult> Post(string configuration)
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            var documents = requestBody.Split(Seperator);

            // setup
            var collection = new RhinoTestCaseCollection();
            collection.Configurations ??= new List<string>();
            collection.Configurations.Add(configuration);
            collection.Id = Guid.NewGuid();

            // parse test cases
            collection.RhinoTestCaseDocuments = documents
                .Select(i => new RhinoTestCaseDocument { Collection = "", RhinoSpec = i })
                .Where(i => !string.IsNullOrEmpty(i.RhinoSpec))
                .ToList();

            // get credentials
            var credentials = Request.GetAuthentication();

            // response
            var data = new { Data = new { Id = repository.Post(credentials, collection) } };
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(data, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.Created.ToInt32()
            };
        }

        // PATCH api/v3/collection/<id>/configurations/<configuration>
        [HttpPatch("{id}/configurations/{configuration}")]
        public IActionResult PatchConfiguration(string id, string configuration)
        {
            // patch
            var (statusCode, _) = repository.Patch(Request.GetAuthentication(), id, configuration);

            // response
            return new ContentResult
            {
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode.ToInt32()
            };
        }

        // PATCH api/v3/collection/<guid>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchScenario(string id)
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            var scenarios = requestBody.Split(Splitter).Select(i => i.Trim());

            // add (generate id)
            var credentials = Request.GetAuthentication();

            // get collection
            var (statusCode, collection) = repository.Get(credentials, id);
            if (statusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            // create entity
            var onScenarios = new List<RhinoTestCaseDocument>();

            // apply
            foreach (var rhinoSpec in scenarios)
            {
                var onScenario = new RhinoTestCaseDocument { Collection = id, Id = Guid.NewGuid(), RhinoSpec = rhinoSpec };
                onScenarios.Add(onScenario);
            }

            if(collection.RhinoTestCaseDocuments == null)
            {
                collection.RhinoTestCaseDocuments = new List<RhinoTestCaseDocument>();
            }
            onScenarios.ForEach(i => collection.RhinoTestCaseDocuments.Add(i));
            repository.Patch(credentials, collection);

            // response
            return NoContent();
        }

        // DELETE api/v3/collection/<guid>
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            // execute
            var response = repository.Delete(Request.GetAuthentication(), id);

            // exit conditions
            return response == HttpStatusCode.NotFound ? NotFound() : (IActionResult)NoContent();
        }

        // DELETE api/v3/collection
        [HttpDelete]
        public IActionResult Delete()
        {
            // execute
            repository.Delete(Request.GetAuthentication());

            // response
            return NoContent();
        }
    }
}