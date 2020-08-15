/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [ApiController]
    public class ConfigurationsController : ControllerBase
    {
        // members: state
        private readonly RhinoConfigurationRepository repository;
        private readonly JsonSerializerSettings jsonSettings;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.ConfigurationsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.ConfigurationsController.</param>
        public ConfigurationsController(IServiceProvider provider)
        {
            repository = provider.GetRequiredService<RhinoConfigurationRepository>();
            jsonSettings = provider.GetRequiredService<JsonSerializerSettings>();
        }

        // GET: api/v3/configuration
        [HttpGet]
        public IActionResult Get()
        {
            // get credentials
            var credentials = Request.GetAuthentication();

            // get response
            var data = repository.Get(credentials).Select(i => new
            {
                Data = new
                {
                    Configurations = new[]
                    {
                        new { Id = $"{i.Id}", Elements = i.Models, Tests = i.TestsRepository }
                    }
                }
            });

            // response
            var responseBody = JsonConvert.SerializeObject(data, jsonSettings);

            // return
            return new ContentResult
            {
                Content = responseBody,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET: api/v3/configuration/<guid>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            // get credentials
            var credentials = Request.GetAuthentication();

            // get data
            var (statusCode, configuration) = repository.Get(credentials, id);

            // exit conditions
            if (statusCode == HttpStatusCode.NotFound.ToInt32())
            {
                return NotFound(new
                {
                    Message = $"Configuration [{id}] was not found."
                });
            }

            // response
            var responseBody = JsonConvert.SerializeObject(configuration, jsonSettings);

            // return
            return new ContentResult
            {
                Content = responseBody,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // POST: api/v3/configuration
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

            // parse test case & configuration
            var configuration = JsonConvert.DeserializeObject<RhinoConfiguration>(requestBody);

            // parse capabilities
            var driverParams = new List<IDictionary<string, object>>();
            foreach (var item in configuration.DriverParameters)
            {
                var driverParam = item;
                if (driverParam.ContainsKey(ContextEntry.Capabilities))
                {
                    var capabilitiesBody = ((JObject)driverParam[ContextEntry.Capabilities]).ToString();
                    driverParam[ContextEntry.Capabilities] =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(capabilitiesBody);
                }
                driverParams.Add(driverParam);
            }
            configuration.DriverParameters = driverParams;

            // get credentials
            var credentials = Request.GetAuthentication();

            // response
            var responseBody = new { Data = new { Id = repository.Post(credentials, configuration) } };
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(responseBody, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = HttpStatusCode.Created.ToInt32()
            };
        }

        // PUT: api/v3/configuration/<guid>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id)
        {
            // read test case from request body
            using var streamReader = new StreamReader(Request.Body);
            var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

            // parse test case & configuration
            var payload = JsonConvert.DeserializeObject<RhinoConfiguration>(requestBody);

            // get credentials
            var credentials = Request.GetAuthentication();

            // get results
            var (statusCode, _) = repository.Put(credentials, id, data: payload);

            // exit conditions
            if (statusCode == HttpStatusCode.NotFound.ToInt32())
            {
                return NotFound(new
                {
                    Message = $"Configuration [{id}] was not found."
                });
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
            return new ContentResult
            {
                Content = default,
                StatusCode = repository.Delete(credentials, id).ToInt32()
            };
        }

        // DELETE: api/v3/configuration
        [HttpDelete]
        public IActionResult Delete()
        {
            // get credentials
            var credentials = Request.GetAuthentication();

            // results
            return new ContentResult
            {
                Content = default,
                StatusCode = repository.Delete(credentials).ToInt32()
            };
        }
    }
}