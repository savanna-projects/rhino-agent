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
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        #region *** GET    ***
        // GET: api/v3/models
        [HttpGet]
        public IActionResult Get()
        {
            // setup
            var responseBody = repository.Get(Request.GetAuthentication()).data.Select(i => new
            {
                i.Id,
                i.Configurations,
                Models = i.Models.Count,
                Entries = i.Models.SelectMany(i=>i.Entries).Count()
            });

            // response
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new { Data = new { Collection = responseBody } }, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET api/v3/models/<id>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            // setup
            var obj = repository.Get(Request.GetAuthentication(), id).data;
            var responseBody = JsonConvert.SerializeObject(obj.Models, jsonSettings);

            // response
            return new ContentResult
            {
                Content = responseBody,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET api/v3/models/<id>/configuration
        [HttpGet("{id}/configurations")]
        public IActionResult GetConfigurations(string id)
        {
            // setup
            var obj = repository.Get(Request.GetAuthentication(), id).data;
            var responseBody = JsonConvert.SerializeObject(new { Data = new { obj.Configurations } }, jsonSettings);

            // response
            return new ContentResult
            {
                Content = responseBody,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }
        #endregion

        #region  *** POST   ***
        // POST api/v3/models
        [HttpPost]
        public Task<IActionResult> Post()
        {
            return DoPost(configuration: string.Empty);
        }

        // POST api/v3/models/<configuration>
        [HttpPost("{configuration}")]
        public Task<IActionResult> Post(string configuration)
        {
            return DoPost(configuration);
        }

        private async Task<IActionResult> DoPost(string configuration)
        {
            // setup
            var modelState = new ModelStateDictionary();

            // read test case from request body
            var models = await Request.ReadAsAsync<RhinoPageModel[]>().ConfigureAwait(false);

            // exit conditions
            if (models.Length == 0)
            {
                modelState.AddModelError("Models.Length", "At least one model must be provided.");
                return BadRequest(modelState);
            }

            // setup
            var collection = new RhinoPageModelCollection();
            collection.Configurations ??= new List<string>();
            collection.Models = models;
            if (!string.IsNullOrEmpty(configuration))
            {
                collection.Configurations.Add(configuration);
            }

            // model name
            foreach (var model in models)
            {
                foreach (var entry in model.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.Model))
                    {
                        continue;
                    }
                    entry.Model = model.Name;
                }
            }

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
        #endregion

        #region *** PATCH  ***
        // PATCH api/v3/models/<id>/configurations/<configuration>
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

        // PATCH api/v3/models/<guid>
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patchmodels(string id)
        {
            // read test case from request body
            var models = await Request.ReadAsAsync<RhinoPageModel[]>().ConfigureAwait(false);

            // add (generate id)
            var credentials = Request.GetAuthentication();

            // get collection
            var (statusCode, collection) = repository.Get(credentials, id);
            if (statusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            // apply
            foreach (var model in models.Where(i => !collection.Models.Select(i => i.Name).Contains(i.Name)))
            {
                collection.Models.Add(model);
            }
            repository.Patch(credentials, collection);

            // response
            return Redirect($"/api/v3/models/{id}");
        }
        #endregion

        #region *** DELETE ***
        // DELETE api/v3/models/<guid>
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            // execute
            var response = repository.Delete(Request.GetAuthentication(), id);

            // exit conditions
            return response == HttpStatusCode.NotFound ? NotFound() : (IActionResult)NoContent();
        }

        // DELETE api/v3/models
        [HttpDelete]
        public IActionResult Delete()
        {
            // execute
            repository.Delete(Request.GetAuthentication());

            // response
            return NoContent();
        }
        #endregion
    }
}
