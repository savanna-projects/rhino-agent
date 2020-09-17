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
using Rhino.Api.Contracts.AutomationProvider;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [Route("api/latest/[controller]")]
    [ApiController]
    public class ModelsController : ControllerBase
    {
        // members: state
        private readonly RhinoModelRepository repository;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.ModelsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.ModelsController.</param>
        public ModelsController(IServiceProvider provider)
        {
            repository = provider.GetRequiredService<RhinoModelRepository>();
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
                Entries = i.Models.SelectMany(i => i.Entries).Count()
            });

            // response
            return this.ContentResult(responseBody: new { Data = new { Collection = responseBody } });
        }

        // GET api/v3/models/<id>
        [HttpGet("{id}")]
        public async Task< IActionResult> Get(string id)
        {
            // setup
            var obj = repository.Get(Request.GetAuthentication(), id).data;

            // exit conditions
            if (obj == default)
            {
                return await this
                    .ErrorResultAsync($"Collection [{id}] was not found.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }

            // response
            return this.ContentResult(responseBody: obj.Models);
        }

        // GET api/v3/models/<id>/configuration
        [HttpGet("{id}/configurations")]
        public async Task<IActionResult> GetConfigurations(string id)
        {
            // setup
            var obj = repository.Get(Request.GetAuthentication(), id).data;

            // exit conditions
            if (obj == default)
            {
                return await this
                    .ErrorResultAsync($"Collection [{id}] was not found", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }

            // response
            return this.ContentResult(new { Data = new { obj.Configurations } });
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
            // read test case from request body
            var models = await Request.ReadAsAsync<RhinoPageModel[]>().ConfigureAwait(false);

            // exit conditions
            if (models.Length == 0)
            {
                return await this
                    .ErrorResultAsync("At least one model must be provided.", HttpStatusCode.BadRequest)
                    .ConfigureAwait(false);
            }
            if (!models.SelectMany(i => i.Entries).Any())
            {
                return await this
                    .ErrorResultAsync("At least one model entry must be provided.", HttpStatusCode.BadRequest)
                    .ConfigureAwait(false);
            }

            // setup
            var collection = new RhinoPageModelCollection();
            collection.Configurations ??= new List<string>();
            collection.Models = models;
            if (!string.IsNullOrEmpty(configuration))
            {
                collection.Configurations.Add(configuration);
            }

            // get credentials
            var credentials = Request.GetAuthentication();

            // response
            var responseBody = new { Data = new { Id = repository.Post(credentials, collection) } };

            // exit conditions
            if (string.IsNullOrEmpty(responseBody.Data.Id))
            {
                return await this
                    .ErrorResultAsync("All the provided Models Collections already exists. Please provide a unique Collection or clean old ones.")
                    .ConfigureAwait(false);
            }

            // results
            return this.ContentResult(responseBody, HttpStatusCode.Created);
        }
        #endregion

        #region *** PATCH  ***
        // PATCH api/v3/models/<id>/configurations/<configuration>
        [HttpPatch("{id}/configurations/{configuration}")]
        public async Task<IActionResult> PatchConfiguration(string id, string configuration)
        {
            // patch
            var (statusCode, _) = repository.Patch(Request.GetAuthentication(), id, configuration);

            // redirect
            if (statusCode == HttpStatusCode.NoContent)
            {
                return Redirect($"/api/v3/models/{id}");
            }

            // response
            return await this
                .ErrorResultAsync("Models collection or configuration were not found.", HttpStatusCode.NotFound)
                .ConfigureAwait(false);
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
                return await this
                    .ErrorResultAsync($"Collection [{id}] was not found.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
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
