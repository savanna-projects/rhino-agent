/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using Microsoft.AspNetCore.Http;

using Rhino.Api.Extensions;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Controllers.Domain.Automation
{
    /// <summary>
    /// Data Access Layer for Rhino API logs repository.
    /// </summary>
    public class LogsRepository : ILogsRepository
    {
        // members: state
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Domain.Automation.LogsRepository.
        /// </summary>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        public LogsRepository(ILogger logger)
        {
            _logger = logger.CreateChildLogger(nameof(LogsRepository));
        }

        #region *** Get    ***
        /// <summary>
        /// Gets a collection of log files on this server.
        /// </summary>
        /// <param name="logPath">The path under which the logs are written.</param>
        /// <returns>A collection of log files.</returns>
        public IEnumerable<string> Get(string logPath)
        {
            // exit conditions
            var logs = Directory.Exists(logPath)
                ? Directory.GetFiles(logPath).Select(i => Path.GetFileName(i))
                : Array.Empty<string>();
            _logger?.Debug($"Get-Logs -LogPath {logPath} = {logs.Count()}");

            // get
            return logs;
        }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <param name="logPath">The path under which the logs are written.</param>
        /// <param name="id">The log id (current date as yyyyMMdd).</param>
        /// <returns>Status code and logs (if any).</returns>
        public Task<(int StatusCode, string LogData)> GetAsync(string logPath, string id)
        {
            return DoGetAsync(logPath, id);
        }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <param name="logPath">The path under which the logs are written.</param>
        /// <param name="id">The log id (current date as yyyyMMdd).</param>
        /// <param name="numberOfLines">A fixed number of lines from the end of the log upwards.</param>
        /// <returns><see cref="int"/> and <see cref="string"/> containing the log's data.</returns>
        public async Task<(int StatusCode, string LogData)> GetAsync(string logPath, string id, int numberOfLines)
        {
            // get
            var (statusCode, logData) = await DoGetAsync(logPath, id).ConfigureAwait(false);

            // exit conditions
            if (statusCode != StatusCodes.Status200OK)
            {
                return (statusCode, logData);
            }

            // parse
            var collection = logData.Split(Environment.NewLine);
            var logs = collection.Skip(Math.Max(0, collection.Length - numberOfLines));
            var logsResult = string.Join(Environment.NewLine, logs);
            _logger?.Debug($"Get-Log -LogPath {logPath} -Id {id} -NumberOfLines {numberOfLines} = Ok");

            // results
            return (StatusCodes.Status200OK, logsResult);
        }

        /// <summary>
        /// Gets a memory stream containing the log's data.
        /// </summary>
        /// <param name="logPath">The path under which the logs were written.</param>
        /// <param name="id">The log id (current date as yyyyMMdd).</param>
        /// <returns><see cref="int"/> and <see cref="MemoryStream"/> containing the log's data.</returns>
        public async Task<(int StatusCode, Stream Stream)> GetAsMemoryStreamAsync(string logPath, string id)
        {
            // get
            var (statusCode, logData) = await DoGetAsync(logPath, id).ConfigureAwait(false);

            // exit conditions
            if (statusCode != StatusCodes.Status200OK)
            {
                return (statusCode, new MemoryStream());
            }

            // build
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(logData ?? ""));
            _logger?.Debug($"Get-LogAsMemoryStream -LogPath {logPath} Id {id} = Ok");

            // get
            return (StatusCodes.Status200OK, memoryStream);
        }

        private async Task<(int StatusCode, string LogData)> DoGetAsync(string logPath, string id)
        {
            // exit conditions
            if (!Directory.Exists(logPath))
            {
                _logger?.Debug($"Get-Log -LogPath {logPath} -Id {id} = NotFound");
                return (StatusCodes.Status404NotFound, string.Empty);
            }

            // parse
            var logsOut = logPath == "."
                ? Path.Join($"{Environment.CurrentDirectory}", "Logs")
                : logPath;
            _logger?.Debug($"Set-LogPath -Path {logsOut} = Ok");

            // get
            var logFile = Path.Join(logsOut, $"RhinoApi-{id}.log");
            if (!File.Exists(path: logFile))
            {
                _logger?.Debug($"Get-Log -LogFile {logFile} = NotFound");
                return (StatusCodes.Status404NotFound, string.Empty);
            }

            // read
            var log = await ControllerUtilities.ForceReadFileAsync(path: logFile).ConfigureAwait(false);
            _logger?.Debug($"Get-Log -LogFile {logFile} = Ok");

            // get
            return (StatusCodes.Status200OK, log);
        }
        #endregion
    }
}