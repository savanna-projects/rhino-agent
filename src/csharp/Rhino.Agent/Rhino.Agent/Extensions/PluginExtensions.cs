/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 * 
 * WORK ITEMS
 * TODO: Expose the option to convert RhinoStep into ActionRule for help display
 * TODO: clean ToActionAttribute extension method
 */
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

using Rhino.Api.Contracts.AutomationProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rhino.Agent.Extensions
{
    internal static class PluginExtensions
    {
        /// <summary>
        /// Converts a RhinoPlugin object into ActionAttribute object.
        /// </summary>
        /// <param name="plugin">RhinoPlugin to convert.</param>
        /// <returns>ActionAttribute object with RhinoPlugin meta data.</returns>
        public static ActionAttribute ToActionAttribute(this RhinoPlugin plugin)
        {
            try
            {
                // setup: CLI arguments
                var cliArguments = plugin
                    .Parameters
                    .Select(i => new KeyValuePair<string, string>(i.Parameter, i.Description))
                    .ToDictionary(i => i.Key, i => i.Value);

                // setup
                var examples = plugin.Examples.Where(i => !string.IsNullOrEmpty(i.Description) && !string.IsNullOrEmpty(i.Example));
                var onExamples = new List<PluginExample>();
                foreach (var rhinoExample in examples)
                {
                    var onExample = new PluginExample
                    {
                        Description = rhinoExample.Description,
                        LiteralExample = rhinoExample.Example,
                        ActionExample = new ActionRule
                        {
                            ActionType = plugin.Key,
                            Argument = Regex.Match(input: rhinoExample.Example, @"{{\$.*}}").Value
                        }
                    };
                    onExamples.Add(onExample);
                }

                // result
                return new ActionAttribute
                {
                    CliArguments = cliArguments,
                    Description = plugin.Scenario,
                    Examples = onExamples.ToArray(),
                    Name = plugin.Key,
                    Summary = plugin.Scenario
                };
            }
            catch (Exception e) when (e !=null)
            {
                // ignore exceptions
            }
            return new ActionAttribute
            {
                CliArguments = new Dictionary<string, string>(),
                Description = string.Empty,
                Examples = Array.Empty<PluginExample>(),
                Name = string.Empty,
                Summary = string.Empty
            };
        }
    }
}