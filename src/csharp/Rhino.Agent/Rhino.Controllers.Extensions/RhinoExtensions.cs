/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Controllers.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for Rhino API related objects.
    /// </summary>
    public static class RhinoExtensions
    {
        // members: data
        private static IDictionary<string, IEnumerable<string>> VerbMap => new Dictionary<string, IEnumerable<string>>
        {
            ["into"] = new[] { ActionType.SendKeys, ActionType.TrySendKeys },
            ["take"] = new[] { ActionType.SelectFromComboBox, ActionType.RegisterParameter, ActionType.GoToUrl },
            ["of"] = new[] { ActionType.GetScreenshot }
        };

        /// <summary>
        /// Converts a RhinoPlugin object into ActionAttribute object.
        /// </summary>
        /// <param name="plugin">RhinoPlugin to convert.</param>
        /// <returns>ActionAttribute object with RhinoPlugin meta data.</returns>
        public static ActionAttribute ToAttribute(this RhinoPlugin plugin)
        {
            try
            {
                // setup: CLI arguments
                var cliArguments = plugin
                    .Parameters
                    .Select(i => new KeyValuePair<string, string>(i.Parameter, i.Description))
                    .ToDictionary(i => i.Key, i => i.Value);

                // setup
                var examples = GetExamples(plugin);

                // result
                return new ActionAttribute
                {
                    CliArguments = cliArguments,
                    Description = plugin.Scenario,
                    Examples = examples.ToArray(),
                    Name = plugin.Key,
                    Summary = plugin.Scenario
                };
            }
            catch (Exception e) when (e != null)
            {
                // ignore exceptions
            }
            return GetDefauleActionAttribute();
        }

        private static IEnumerable<PluginExample> GetExamples(RhinoPlugin plugin)
        {
            // setup
            var examples = plugin.Examples.Where(i => !string.IsNullOrEmpty(i.Description) && !string.IsNullOrEmpty(i.Example));
            var onExamples = new List<PluginExample>();

            // iterate
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

            // get
            return onExamples;
        }

        private static ActionAttribute GetDefauleActionAttribute() => new ActionAttribute
        {
            CliArguments = new Dictionary<string, string>(),
            Description = string.Empty,
            Examples = Array.Empty<PluginExample>(),
            Name = string.Empty,
            Summary = string.Empty
        };

        /// <summary>
        /// Converts a ActionAttribute object into ActionModel object.
        /// </summary>
        /// <param name="attribute">ActionAttribute to convert.</param>
        /// <param name="source"> the source of the plugin (e.g. code, plugin, etc.).</param>
        /// <returns>ActionModel object with ActionAttribute meta data.</returns>
        public static ActionModel ToModel(this ActionAttribute attribute, string source)
        {
            try
            {
                // build
                return new ActionModel
                {
                    Key = attribute.Name,
                    Literal = attribute.Name.ToSpaceCase().ToLower(),
                    Verb = GetVerb(attribute.Name),
                    Entity = attribute.ToSerializable(),
                    Source = source
                };
            }
            catch (Exception e) when (e != null)
            {
                // ignore exceptions
            }
            return default;
        }

        /// <summary>
        /// Converts a AssertMethodAttribute object into AssertModel object.
        /// </summary>
        /// <param name="attribute">AssertMethodAttribute to convert.</param>
        /// <returns>AssertModel object with AssertMethodAttribute meta data.</returns>
        public static AssertModel ToModel(this AssertMethodAttribute attribute)
        {
            try
            {
                // build
                return new AssertModel
                {
                    Key = attribute.Name,
                    Literal = attribute.Name.ToPascalCase().ToSpaceCase().ToLower(),
                    Verb = GetVerb(attribute.Name),
                    Entity = attribute.ToSerializable()
                };
            }
            catch (Exception e) when (e != null)
            {
                // ignore exceptions
            }
            return default;
        }

        /// <summary>
        /// Converts a ConnectorAttribute object into ConnectorModel object.
        /// </summary>
        /// <param name="attribute">ConnectorAttribute to convert.</param>
        /// <returns>ConnectorModel object with ConnectorAttribute meta data.</returns>
        public static ConnectorModel ToModel(this ConnectorAttribute attribute)
        {
            try
            {
                // build
                return new ConnectorModel
                {
                    Key = attribute.Value,
                    Literal = attribute.Value.ToPascalCase().ToSpaceCase().ToLower(),
                    Verb = string.Empty,
                    Entity = attribute.ToSerializable()
                };
            }
            catch (Exception e) when (e != null)
            {
                // ignore exceptions
            }
            return default;
        }

        /// <summary>
        /// Converts a MacroAttribute object into MacroModel object.
        /// </summary>
        /// <param name="attribute">MacroAttribute to convert.</param>
        /// <returns>MacroModel object with MacroAttribute meta data.</returns>
        public static MacroModel ToModel(this MacroAttribute attribute)
        {
            try
            {
                // build
                return new MacroModel
                {
                    Key = attribute.Name,
                    Literal = attribute.Name.ToSpaceCase().ToLower(),
                    Verb = string.Empty,
                    Entity = attribute.ToSerializable()
                };
            }
            catch (Exception e) when (e != null)
            {
                // ignore exceptions
            }
            return default;
        }

        /// <summary>
        /// Converts a ReporterAttribute object into ReporterModel object.
        /// </summary>
        /// <param name="attribute">ReporterAttribute to convert.</param>
        /// <returns>ReporterModel object with ReporterAttribute meta data.</returns>
        public static ReporterModel ToModel(this ReporterAttribute attribute)
        {
            try
            {
                // build
                return new ReporterModel
                {
                    Key = attribute.Name,
                    Literal = attribute.Name.ToPascalCase().ToSpaceCase().ToLower(),
                    Verb = string.Empty,
                    Entity = attribute.ToSerializable()
                };
            }
            catch (Exception e) when (e != null)
            {
                // ignore exceptions
            }
            return default;
        }

        // gets a verb for this action from default verbs map
        private static string GetVerb(string action)
        {
            return VerbMap.FirstOrDefault(i => i.Value.Contains(action)).Key ?? "on";
        }
    }
}