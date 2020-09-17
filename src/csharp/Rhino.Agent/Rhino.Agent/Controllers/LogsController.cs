/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Rhino.Agent.Domain;
using Rhino.Agent.Extensions;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [Route("api/latest/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        // members: state
        private readonly RhinoLogsRepository repository;
        private readonly string logPath;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Controllers.LogsController.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Controllers.LogsController.</param>
        public LogsController(IServiceProvider provider, IConfiguration appSettings)
        {
            repository = provider.GetRequiredService<RhinoLogsRepository>();

            // get in-folder
            var inFolder = appSettings.GetValue<string>("rhino:reportConfiguration:logsOut");
            logPath = string.IsNullOrEmpty(inFolder) ? Environment.CurrentDirectory + "/Logs" : inFolder;
        }

        // GET: api/v3/logs/<log>
        [HttpGet("{log}")]
        public IActionResult Get(string log)
        {
            // get
            var (statusCode, responseBody) = repository.Get(logPath, log);

            // exit conditions
            if (statusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new { Message = $"Log [{log}] or configuration [{logPath}] were not found." });
            }

            // response
            return this.ContentTextResult(responseBody, HttpStatusCode.OK);
        }

        // GET: api/v3/logs/<log>/size/<size>
        [HttpGet("{log}/size/{size}")]
        public IActionResult Get(string log, int size)
        {
            return this.ContentTextResult(
                responseBody: repository.Get(logPath, log, size).data,
                statusCode: HttpStatusCode.OK);
        }

        // GET: api/v3/logs/<log>/download
        [HttpGet("{log}/download")]
        public async Task<IActionResult> Download(string log)
        {
            // setup
            var logName = $"RhinoApi-{log}";
            var fullLogName = logName + ".log";

            // get report
            var (statusCode, stream) = repository.GetAsMemoryStream(logPath, log);

            // exit conditions
            if (statusCode == HttpStatusCode.NotFound)
            {
                return await this
                    .ErrorResultAsync($"Log [{fullLogName}] under configuration [{logPath}] was not found.", HttpStatusCode.NotFound)
                    .ConfigureAwait(false);
            }

            // response
            var zipContent = stream.Zip(logName);
            return File(zipContent, MediaTypeNames.Application.Zip, logName + ".zip");
        }
    }
}