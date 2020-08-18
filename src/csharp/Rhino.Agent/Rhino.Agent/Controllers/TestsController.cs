/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
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
using Rhino.Api.Parser.Contracts;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        // members: constants
        private readonly string Seperator =
            Environment.NewLine + Environment.NewLine + SpecSection.Separator + Environment.NewLine + Environment.NewLine;
        private readonly string Splitter = ">>>";
        private const string CountHeader = "Rhino-Total-Specs";

        // members: state
        private readonly RhinoTestCaseRepository rhinoTest;
        private readonly RhinoConfigurationRepository rhinoConfiguration;
        private readonly JsonSerializerSettings jsonSettings;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.TestsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.ModelsController.</param>
        public TestsController(IServiceProvider provider)
        {
            rhinoTest = provider.GetRequiredService<RhinoTestCaseRepository>();
            rhinoConfiguration = provider.GetRequiredService<RhinoConfigurationRepository>();
            jsonSettings = provider.GetRequiredService<JsonSerializerSettings>();
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
            Response.Headers.Add(CountHeader, $"{data.Select(i=> i.Tests).Sum()}");

            // response
            var responseBody = JsonConvert.SerializeObject(new { Data = new { Collections = data } }, jsonSettings);
            return new ContentResult
            {
                Content = responseBody,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET api/v3/tests/<id>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            // setup
            var obj = rhinoTest.Get(Request.GetAuthentication(), id).data;

            // exit conditions
            if (obj == default)
            {
                return NotFound(new { Message = $"Collection [{id}] was not found." });
            }

            // setup
            var specs = obj.RhinoTestCaseDocuments.Select(i => i.RhinoSpec);
            var responseBody = string.Join(Seperator, specs);

            // add count header
            Response.Headers.Add(CountHeader, $"{specs.Count()}");

            // response
            return new ContentResult
            {
                Content = responseBody,
                ContentType = MediaTypeNames.Text.Plain,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET api/v3/tests/<id>/configuration
        [HttpGet("{id}/configurations")]
        public IActionResult GetConfigurations(string id)
        {
            // setup
            var (statusCode, data) = rhinoTest.Get(Request.GetAuthentication(), id);
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

        // TODO: clean
        private async Task<IActionResult> DoPost(string configuration)
        {
            // read test case from request body
            var requestBody = await Request.ReadAsync().ConfigureAwait(false);

            // exit conditions
            if (string.IsNullOrEmpty(requestBody))
            {
                return GetErrorResults(message: "You must provide at least one test case.");
            }

            var documents = requestBody.Split(Seperator);

            // setup
            var collection = new RhinoTestCaseCollection();
            collection.Configurations ??= new List<string>();
            if (!string.IsNullOrEmpty(configuration))
            {
                var (statusCode, _) = rhinoConfiguration.Get(Request.GetAuthentication(), configuration);
                if(statusCode == HttpStatusCode.NotFound)
                {
                    return NotFound(new { Message = $"Configuration [{configuration}] was not found." });
                }
                collection.Configurations.Add(configuration);
            }
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
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(data, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.Created.ToInt32()
            };
        }
        #endregion

        #region *** PATCH  ***
        // PATCH api/v3/tests/<id>/configurations/<configuration>
        [HttpPatch("{id}/configurations/{configuration}")]
        public IActionResult PatchConfiguration(string id, string configuration)
        {
            // patch
            var (statusCode, _) = rhinoTest.Patch(Request.GetAuthentication(), id, configuration);

            // exit conditions
            if (statusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new { Message = $"Collection [{id}] or Configuration [{configuration}] were not found." });
            }
            if (statusCode == HttpStatusCode.BadRequest)
            {
                return GetErrorResults("You must provide configuration ID in the request route.");
            }

            // response
            return new ContentResult
            {
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode.ToInt32()
            };
        }

        // PATCH api/v3/tests/<guid>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchTestCases(string id)
        {
            // read test case from request body
            var requestBody = await Request.ReadAsync().ConfigureAwait(false);
            var specs = requestBody.Split(Splitter).Select(i => i.Trim());

            // add (generate id)
            var credentials = Request.GetAuthentication();

            // get collection
            var (statusCode, collection) = rhinoTest.Get(credentials, id);
            if (statusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new { Message = $"Collection [{id}] was not found." });
            }
            if(statusCode == HttpStatusCode.BadRequest)
            {
                return GetErrorResults("You must provide at least one test case.");
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
        public IActionResult Delete(string id)
        {
            // execute
            var response = rhinoTest.Delete(Request.GetAuthentication(), id);

            // exit conditions
            return response == HttpStatusCode.NotFound
                ? NotFound(new { Message = $"Collection [{id}] was not found." })
                : (IActionResult)NoContent();
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

        private ContentResult GetErrorResults(string message)
        {
            // setup
            var obj = new
            {
                Message = message
            };

            // response
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(obj, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.BadRequest.ToInt32()
            };
        }
    }
}