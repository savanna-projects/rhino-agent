/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rhino.Api.Contracts.AutomationProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rhino.Agent.Extensions
{
    /// <summary>
    /// Internal Utilities package.
    /// </summary>
    public static class Utilities
    {
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

        /// <summary>
        /// Gets a list of available reports created by automation runs.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> by which to fetch settings.</param>
        /// <returns>A list of reports.</returns>
        public static IEnumerable<(string Path, string Name)> GetReports(IConfiguration configuration)
        {
            // setup
            var path = DoGetStaticReportsFolder(configuration);
            var reports = Directory.GetDirectories(path).Select(i => (i, Path.GetFileName(i)));

            return reports;
        }

        /// <summary>
        /// Gets the static reports folder in which static reports can be served.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> by which to fetch settings.</param>
        /// <returns>Servable static repotrs folder.</returns>
        public static string GetStaticReportsFolder(IConfiguration configuration)
        {
            return DoGetStaticReportsFolder(configuration);
        }

        // INTERNAL
        private static string DoGetStaticReportsFolder(IConfiguration configuration)
        {
            // setup
            var onFolder = configuration.GetValue("rhino:reportConfiguration:reportOut", ".");
            onFolder = Path.GetFileName(onFolder);
            onFolder = onFolder == "." ? Path.Join(Environment.CurrentDirectory, "outputs", "reports") : onFolder;

            // get
            return Path.IsPathRooted(onFolder) ? onFolder : Path.Join(Environment.CurrentDirectory, onFolder);
        }
    }
}