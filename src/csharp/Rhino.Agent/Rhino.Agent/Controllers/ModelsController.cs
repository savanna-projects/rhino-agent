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
using Rhino.Api.Contracts.AutomationProvider;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [ApiController]
    public class ModelsController : ControllerBase
    {
        // members: state
        private readonly RhinoModelRepository repository;
        private readonly JsonSerializerSettings jsonSettings;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.ModelsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.ModelsController.</param>
        public ModelsController(IServiceProvider provider)
        {
            repository = provider.GetRequiredService<RhinoModelRepository>();
            jsonSettings = provider.GetRequiredService<JsonSerializerSettings>();
        }

        // GET: api/v3/elements
        [HttpGet]
        public IActionResult Get()
        {
            // setup
            var data = repository.Get(Request.GetAuthentication()).data.Select(i => new
            {
                i.Id,
                i.Configurations,
                TotalElements = i.Models.Count
            });

            // response
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new { Data = new { Collections = data } }, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET api/v3/elements/<id>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            // setup
            var data = repository.Get(Request.GetAuthentication(), id).data;
            var body = JsonConvert.SerializeObject(data.Models, jsonSettings);

            // response
            return new ContentResult
            {
                Content = body,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET api/v3/elements/<id>/configuration
        [HttpGet("{id}/configurations")]
        public IActionResult GetConfigurations(string id)
        {
            // setup
            var data = repository.Get(Request.GetAuthentication(), id).data;

            // response
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new { Data = new { data.Configurations } }, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // POST api/v3/elements/<configuration>
        [HttpPost("{configuration}")]
        public async Task<IActionResult> Post(string configuration)
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            var models = JsonConvert.DeserializeObject<RhinoPageModel[]>(requestBody);

            // exit conditions
            if (models.Length == 0)
            {
                return BadRequest();
            }

            // setup
            var collection = new RhinoPageModelCollection();
            collection.Configurations ??= new List<string>();
            collection.Configurations.Add(configuration);
            collection.Id = Guid.NewGuid();
            collection.Models = models;

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

        // PATCH api/v3/elements/<id>/configurations/<configuration>
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
        public async Task<IActionResult> PatchElements(string id)
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            var models =
                JsonConvert.DeserializeObject<RhinoPageModel[]>(requestBody, jsonSettings);

            // add (generate id)
            var credentials = Request.GetAuthentication();

            // get collection
            var (statusCode, collection) = repository.Get(credentials, id);
            if (statusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            // apply
            foreach (var model in models)
            {
                collection.Models.Add(model);
            }
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
