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

            // exit conditions
            if (obj == default)
            {
                return NotFound(new { Message = $"Collection [{id}] was not found." });
            }

            // response
            var responseBody = JsonConvert.SerializeObject(obj.Models, jsonSettings);
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

            // exit conditions
            if (obj == default)
            {
                return NotFound(new { Message = $"Collection [{id}] was not found" });
            }

            // response
            var responseBody = JsonConvert.SerializeObject(new { Data = new { obj.Configurations } }, jsonSettings);
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

        // TODO: clean
        private async Task<IActionResult> DoPost(string configuration)
        {
            // read test case from request body
            var models = await Request.ReadAsAsync<RhinoPageModel[]>().ConfigureAwait(false);

            // exit conditions
            if (models.Length == 0)
            {
                return GetErrorResults(message: "At least one model must be provided.");
            }
            if (!models.SelectMany(i => i.Entries).Any())
            {
                return GetErrorResults(message: "At least one model entry must be provided.");
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

            // exit conditions
            if (string.IsNullOrEmpty(data.Data.Id))
            {
                return GetErrorResults(
                    message: "All the provided Models Collections already exists. Please provide a unique Collection.");
            }

            // results
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

            // redirect
            if(statusCode == HttpStatusCode.NoContent)
            {
                return Redirect($"/api/v3/models/{id}");
            }

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
            // setup
            var models = await Request.ReadAsAsync<RhinoPageModel[]>().ConfigureAwait(false);

            // add (generate id)
            var credentials = Request.GetAuthentication();

            // get collection
            var (statusCode, collection) = repository.Get(credentials, id);
            if (statusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new { Message = $"Collection [{id}] was not found." });
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
