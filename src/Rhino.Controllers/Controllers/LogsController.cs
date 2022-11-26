/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Rhino.Controllers.Domain;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion($"{AppSettings.ApiVersion}.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        // members: state
        private readonly IDomain _domain;
        private readonly string _logPath;

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="domain">An ILogsRepository implementation to use with the Controller.</param>
        public LogsController(IDomain domain)
        {
            _domain = domain;

            // get in-folder
            var inFolder = domain.AppSettings.ReportsAndLogs.LogsOut;
            _logPath = string.IsNullOrEmpty(inFolder) ? ControllerUtilities.LogsDefaultFolder : inFolder;
        }

        #region *** Get    ***
        // GET: api/v3/logs
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get-Log -All",
            Description = "Returns an existing _**Automation Logs**_ files list.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Get()
        {
            // get
            var responseBody = _domain.Logs.Get(_logPath);

            // response
            return Ok(responseBody);
        }

        // GET: api/v3/logs/:id
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get-Log -Id {yyyy-MM-dd.log}",
            Description = "Returns an existing _**Automation Log**_.")]
        [Produces(MediaTypeNames.Text.Plain, MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Get([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get
            var (statusCode, responseBody) = await _domain.Logs.GetAsync(_logPath, id).ConfigureAwait(false);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Log -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // response
            return Ok(responseBody);
        }

        // GET: api/v3/logs/:id/size/:size
        [HttpGet("{id}/size/{size}")]
        [SwaggerOperation(
            Summary = "Get-Log -Id {yyyy-MM-dd.log} -Size 20",
            Description = "Returns an existing _**Automation Log**_ tail, by specific size.")]
        [Produces(MediaTypeNames.Text.Plain, MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Get(
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string id,
            [SwaggerParameter("A fixed number of lines from the end of the log upwards.")] int size)
        {
            // setup
            var (statusCode, responseBody) = await _domain.Logs.GetAsync(_logPath, id, size).ConfigureAwait(false);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Log -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // get
            return Ok(responseBody);
        }

        // GET: api/v3/logs/:id/export
        [HttpGet("{id}/export")]
        [SwaggerOperation(
            Summary = "Export-Log -Id {yyyy-MM-dd.log}",
            Description = "Returns an existing _**Automation Log**_.")]
        [Produces(MediaTypeNames.Text.Plain, MediaTypeNames.Application.Json, "application/force-download")]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Export([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // setup
            var logName = $"RhinoApi-{id}";
            var fullLogName = logName + ".log";

            // exit conditions
            if (!Directory.Exists(_logPath))
            {
                return NotFound();
            }

            // parse
            var logsOut = _logPath == "." ? ControllerUtilities.LogsDefaultFolder : _logPath;

            // get
            var logFile = Path.Join(logsOut, $"RhinoApi-{id}.log");
            if (!System.IO.File.Exists(path: logFile))
            {
                return NotFound();
            }

            // build
            var log = await ControllerUtilities.ForceReadFileAsync(logFile).ConfigureAwait(false);
            var bytes = Encoding.UTF8.GetBytes(log);

            // get
            return File(bytes, "application/force-download", fullLogName);
        }
        #endregion
    }
}
