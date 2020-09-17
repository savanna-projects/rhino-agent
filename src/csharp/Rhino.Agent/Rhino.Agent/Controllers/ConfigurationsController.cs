/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;
using Rhino.Api.Contracts.Configuration;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [Route("api/latest/[controller]")]
    [ApiController]
    public class ConfigurationsController : ControllerBase
    {
        // members: state
        private readonly RhinoConfigurationRepository repository;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.ConfigurationsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.ConfigurationsController.</param>
        public ConfigurationsController(IServiceProvider provider)
        {
            repository = provider.GetRequiredService<RhinoConfigurationRepository>();
        }

        // GET: api/v3/configuration
        [HttpGet]
        public IActionResult Get()
        {
            // get credentials
            var credentials = Request.GetAuthentication();

            // get response
            var responseBody = repository.Get(credentials).Select(i => new
            {
                Data = new
                {
                    Configurations = new[]
                    {
                        new
                        {
                            Id = $"{i.Id}",
                            Elements = i.Models,
                            Tests = i.TestsRepository
                        }
                    }
                }
            });

            // return
            return this.ContentResult(responseBody);
        }

        // GET: api/v3/configuration/<guid>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            // get credentials
            var credentials = Request.GetAuthentication();

            // get data
            var (statusCode, configuration) = repository.Get(credentials, id);

            // exit conditions
            if (statusCode == HttpStatusCode.NotFound)
            {
                return await this
                    .ErrorResultAsync($"Configuration [{id}] was not found.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return this.ContentResult(responseBody: configuration);
        }

        // POST: api/v3/configuration
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            // parse test case & configuration
            var configuration = await Request.ReadAsAsync<RhinoConfiguration>().ConfigureAwait(false);

            // exit conditions
            if (!configuration.DriverParameters.Any())
            {
                return await this
                    .ErrorResultAsync("You must provide at least one driver parameter.")
                    .ConfigureAwait(false);
            }

            // parse driver parameters
            configuration.DriverParameters = Utilities.ParseDriverParameters(configuration.DriverParameters);

            // get credentials
            var credentials = Request.GetAuthentication();

            // response
            var responseBody = new { Data = new { Id = repository.Post(credentials, configuration) } };
            return this.ContentResult(responseBody, HttpStatusCode.Created);
        }

        // PUT: api/v3/configuration/<guid>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id)
        {
            // parse test case & configuration
            var configuration = await Request.ReadAsAsync<RhinoConfiguration>().ConfigureAwait(false);

            // exit conditions
            if (!configuration.DriverParameters.Any())
            {
                return await this
                    .ErrorResultAsync("You must provide at least one driver parameter.")
                    .ConfigureAwait(false);
            }

            // get credentials
            var credentials = Request.GetAuthentication();

            // get results
            var (statusCode, _) = repository.Put(credentials, id, data: configuration);

            // exit conditions
            if (statusCode == HttpStatusCode.NotFound)
            {
                return await this
                    .ErrorResultAsync($"Configuration [{id}] was not found.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }

            // response
            return Redirect($"/api/v3/configurations/{id}");
        }

        // DELETE: api/v3/configuration/<guid>
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            // get credentials
            var credentials = Request.GetAuthentication();

            // results
            return this.ContentResult(
                responseBody: default,
                statusCode: repository.Delete(credentials, id));
        }

        // DELETE: api/v3/configuration
        [HttpDelete]
        public IActionResult Delete()
        {
            // get credentials
            var credentials = Request.GetAuthentication();

            // results
            return this.ContentResult(
                responseBody: default,
                statusCode: repository.Delete(credentials));
        }
    }
}