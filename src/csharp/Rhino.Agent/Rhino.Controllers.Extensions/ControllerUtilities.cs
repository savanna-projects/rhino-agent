/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rhino.Api.Contracts.AutomationProvider;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rhino.Controllers.Extensions
{
    public static class ControllerUtilities
    {
        // constants
        public const string LogsConfigurationKey = "Rhino:ReportConfiguration:LogsOut";
        public const string ReportsConfigurationKey = "Rhino:ReportConfiguration:ReportsOut";

        /// <summary>
        /// Gets the default logs output folder path (not including the log name);
        /// </summary>
        public static string LogsDefaultFolder => GetLogsDefaultFolder();

        /// <summary>
        /// Normalize driver parameters to match Gravity's driver parameters contract.
        /// </summary>
        /// <param name="driverParameters">Driver parameters to normalize.</param>
        /// <returns>Normalized driver parameters.</returns>
        public static IEnumerable<IDictionary<string, object>> ParseDriverParameters(IEnumerable<IDictionary<string, object>> driverParameters)
        {
            // setup
            var onDriverParameters = new List<IDictionary<string, object>>();

            // iterate
            foreach (var item in driverParameters)
            {
                var driverParam = item;
                if (driverParam.ContainsKey(ContextEntry.Capabilities))
                {
                    var capabilitiesBody = ((JObject)driverParam[ContextEntry.Capabilities]).ToString();
                    driverParam[ContextEntry.Capabilities] =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(capabilitiesBody);
                }
                onDriverParameters.Add(driverParam);
            }

            // results
            return onDriverParameters;
        }

        #region *** Logger  ***
        /// <summary>
        /// Gets a new instance of a default ILogger with TraceLogger implementation.
        /// </summary>
        /// <param name="configuration">Configuration by which to created log files and log level (optional).</param>
        /// <returns>An ILogger instance.</returns>
        public static ILogger GetLogger(IConfiguration configuration)
        {
            return DoGetLogger(configuration);
        }

        /// <summary>
        /// Gets a new instance of a default ILogger with TraceLogger implementation.
        /// </summary>
        /// <param name="type">The logger type.</param>
        /// <returns>An ILogger instance.</returns>
        public static ILogger GetLogger(Type type)
        {
            // constants
            const string File = "appsettings.json";
            var name = type.Name;
            var root = Environment.CurrentDirectory;

            // setup
            var configuration = new ConfigurationBuilder().SetBasePath(root).AddJsonFile(File).Build();

            // build
            return DoGetLogger(configuration).CreateChildLogger(name);
        }

        private static ILogger DoGetLogger(IConfiguration configuration)
        {
            // get in folder
            var inFolder = configuration.GetValue<string>(LogsConfigurationKey);
            inFolder = string.IsNullOrEmpty(inFolder) ? GetLogsDefaultFolder() : inFolder;

            // setup logger
            return new TraceLogger(applicationName: "RhinoApi", loggerName: string.Empty, inFolder);
        }
        #endregion

        #region *** Reports ***
        /// <summary>
        /// Gets a list of available reports created by automation runs.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> by which to fetch settings.</param>
        /// <returns>A list of reports.</returns>
        public static IEnumerable<(string Path, string Name)> GetReports(IConfiguration configuration)
        {
            // setup
            var path = DoGetStaticReportsFolder(configuration);

            // get
            return Directory.GetDirectories(path).Select(i => (i, Path.GetFileName(i)));
        }

        /// <summary>
        /// Gets the static reports folder in which static reports can be served.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> by which to fetch settings.</param>
        /// <returns>Static reports folder.</returns>
        public static string GetStaticReportsFolder(IConfiguration configuration)
        {
            return DoGetStaticReportsFolder(configuration);
        }

        private static string DoGetStaticReportsFolder(IConfiguration configuration)
        {
            // setup
            var onFolder = configuration.GetValue(ReportsConfigurationKey, ".");

            // is current location
            if (onFolder == ".")
            {
                onFolder = Path.Join(Environment.CurrentDirectory, "Outputs", "Reports", "rhino");
            }
            onFolder = onFolder.Replace(Path.GetFileName(onFolder), string.Empty);

            // setup
            return Path.IsPathRooted(onFolder) ? onFolder : Path.Join(Environment.CurrentDirectory, onFolder);
        }
        #endregion

        /// <summary>
        /// Force a file reading even if the file is open by another process.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A string containing all the text in the file.</returns>
        public static async Task<string> ForceReadFileAsync(string path)
        {
            // force open
            var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // build
            var reader = new StreamReader(stream);

            // read
            var log = await reader.ReadToEndAsync().ConfigureAwait(false);

            // cleanup
            reader.Dispose();
            await stream.DisposeAsync().ConfigureAwait(false);

            // get
            return log;
        }

        /// <summary>
        /// Gets the default JsonSerializerSettings (prettify and camelCase)
        /// </summary>
        public static JsonSerializerOptions JsonSettings => new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };        

        // Utilities
        private static string GetLogsDefaultFolder() => Path.Join(Environment.CurrentDirectory, "Logs");
    }
}