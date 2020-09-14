/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Rhino.Agent.Domain
{
    /// <summary>
    /// Data Access Layer for Rhino API logs repository.
    /// </summary>
    public class RhinoLogsRepository : Repository
    {
        // members: state
        private readonly RhinoConfigurationRepository configurationRepository;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Domain.RhinoTestCaseRepository.</param>
        public RhinoLogsRepository(IServiceProvider provider)
            : base(provider)
        {
            configurationRepository = provider.GetRequiredService<RhinoConfigurationRepository>();
        }

        #region *** GET    ***
        /// <summary>
        /// GET logs from this domain state.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get logs.</param>
        /// <param name="configuration">The configuration id by which to GET.</param>
        /// <param name="log">The log id (current date as yyyyMMdd).</param>
        /// <returns>Status code and logs (if any).</returns>
        public (HttpStatusCode statusCode, string data) Get(string configuration, string log)
        {
            return DoGet(configuration, log);
        }

        /// <summary>
        /// GET logs from this domain state.
        /// </summary>
        /// <param name="logPath">The path under which the logs are written.</param>
        /// <param name="log">The log id (current date as yyyyMMdd).</param>
        /// <param name="size">A fixed number of lines from the end of the log upwards.</param>
        /// <returns>Status code and logs (if any).</returns>
        public (HttpStatusCode statusCode, string data) Get(string logPath, string log, int size)
        {
            // get
            var (statusCode, data) = DoGet(logPath, log);

            // exit conditions
            if (statusCode != HttpStatusCode.OK)
            {
                return (statusCode, data);
            }

            // parse
            var collection = data.Split(Environment.NewLine);
            var logs = collection.Skip(Math.Max(0, collection.Length - size));

            // results
            return (HttpStatusCode.OK, string.Join(Environment.NewLine, logs));
        }

        /// <summary>
        /// GET a zip file contains test run report.
        /// </summary>
        /// <param name="logPath">The path under which the logs are written.</param>
        /// <param name="log">The log id (current date as yyyyMMdd).</param>
        /// <returns>Status code and memory stream.</returns>
        public (HttpStatusCode statusCode, MemoryStream stream) GetAsMemoryStream(
            string logPath,
            string log)
        {
            // get
            var (statusCode, data) = DoGet(logPath, log);

            // exit conditions
            if (statusCode != HttpStatusCode.OK)
            {
                return (statusCode, new MemoryStream());
            }

            // results
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(data ?? ""));
            return (HttpStatusCode.OK, memoryStream);
        }

        private (HttpStatusCode statusCode, string data) DoGet(string logsPath, string log)
        {
            // exit conditions
            if (!Directory.Exists(logsPath))
            {
                return (HttpStatusCode.NotFound, string.Empty);
            }

            // parse
            var logsOut = logsPath == "." ? $"{Environment.CurrentDirectory}\\logs\\" : logsPath;
            logsOut = logsOut.EndsWith("\\") ? logsOut : logsOut + "\\";

            // get
            var logFile = $"{logsOut}RhinoApi-{log}.log";
            if (!File.Exists(path: logFile))
            {
                return (HttpStatusCode.NotFound, string.Empty);
            }

            // read
            using var reader = new StreamReader(path: logFile, Encoding.UTF8);
            return (HttpStatusCode.OK, reader.ReadToEnd());
        }
        #endregion
    }
}