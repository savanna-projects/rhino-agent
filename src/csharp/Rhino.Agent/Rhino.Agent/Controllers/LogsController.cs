/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Net;
using System.Net.Mime;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        // members: state
        private readonly RhinoLogsRepository repository;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.LogsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.LogsController.</param>
        public LogsController(IServiceProvider provider)
        {
            repository = provider.GetRequiredService<RhinoLogsRepository>();
        }

        // GET: api/v3/logs/<log>/configuration/<configuration>
        [HttpGet("{log}/configuration/{configuration}")]
        public IActionResult Get(string configuration, string log)
        {
            // get credentials
            var credentials = Request.GetAuthentication();

            // get
            var (statusCode, responseBody) = repository.Get(credentials, configuration, log);

            // exit conditions
            if(statusCode== HttpStatusCode.NotFound)
            {
                return NotFound(new { Message = $"Log [{log}] or configuration [{configuration}] were not found." });
            }

            // response
            return new ContentResult
            {
                Content = responseBody,
                ContentType = MediaTypeNames.Text.Plain,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET: api/v3/logs/<log>/configuration/<configuration>/size/<size>
        [HttpGet("{log}/configuration/{configuration}/size/{size}")]
        public IActionResult Get(string configuration, string log, int size)
        {
            // get credentials
            var credentials = Request.GetAuthentication();

            // response
            return new ContentResult
            {
                Content = repository.Get(credentials, configuration, log, size).data,
                ContentType = MediaTypeNames.Text.Plain,
                StatusCode = HttpStatusCode.OK.ToInt32()
            };
        }

        // GET: api/v3/logs/<log>/configuration/<configuration>/download
        [HttpGet("{log}/configuration/{configuration}/download")]
        public IActionResult Download(string configuration, string log)
        {
            // get credentials
            var credentials = Request.GetAuthentication();

            // setup
            var logName = $"Rhino-{log}.log";

            // get report
            var (statusCode, stream) = repository.GetAsMemoryStream(credentials, configuration, log);

            // exit conditions
            if (statusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new
                {
                    Message = $"Log [{logName}] under configuration [{configuration}] was not found."
                });
            }

            // response
            var zipContent = stream.Zip(logName);
            return File(zipContent, MediaTypeNames.Application.Zip, logName);
        }
    }
}