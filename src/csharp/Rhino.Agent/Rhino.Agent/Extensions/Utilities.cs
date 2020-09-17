/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rhino.Api.Contracts.AutomationProvider;

using System.Collections.Generic;

namespace Rhino.Agent.Extensions
{
    /// <summary>
    /// Internal Utilities package.
    /// </summary>
    internal static class Utilities
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
    }
}