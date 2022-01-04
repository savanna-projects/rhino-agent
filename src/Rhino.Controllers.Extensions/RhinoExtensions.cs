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
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Interfaces;
using Rhino.Controllers.Models;

using System.Data;
using System.Text.Json;
using System.Reflection;
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

        private static ActionAttribute GetDefauleActionAttribute() => new()
        {
            CliArguments = new Dictionary<string, string>(),
            Description = string.Empty,
            Examples = Array.Empty<PluginExample>(),
            Name = string.Empty,
            Summary = string.Empty
        };

        #region *** Connector  ***
        /// <summary>
        /// Gets a connector.
        /// </summary>
        /// <param name="configuration">RhinoConfiguration by which to factor RhinoConnector</param>
        /// <returns>RhinoConnector implementation.</returns>
        public static Type GetConnector(this RhinoConfiguration configuration)
        {
            return DoGetConnector(configuration, Utilities.Types);
        }

        /// <summary>
        /// Gets a connector.
        /// </summary>
        /// <param name="configuration">RhinoConfiguration by which to factor RhinoConnector</param>
        /// <param name="types">A collection of <see cref="Type>"/> in which to search for RhinoConnector.</param>
        /// <returns>RhinoConnector implementation.</returns>
        public static Type GetConnector(this RhinoConfiguration configuration, IEnumerable<Type> types)
        {
            return DoGetConnector(configuration, types);
        }

        private static Type DoGetConnector(RhinoConfiguration configuration, IEnumerable<Type> types)
        {
            // constants
            const StringComparison C = StringComparison.OrdinalIgnoreCase;

            // types loading pipeline
            var byContract = types.Where(t => typeof(IConnector).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
            var byAttribute = byContract.Where(t => t.GetCustomAttribute<ConnectorAttribute>() != null);

            // get connector type by it's name
            var type = byAttribute
                .FirstOrDefault(t => t.GetCustomAttribute<ConnectorAttribute>().Value.Equals(configuration.ConnectorConfiguration.Connector, C));

            if (type == default)
            {
                return default;
            }

            // activate new connector instance
            return type;
        }
        #endregion

        #region *** Models     ***
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
                    Entity = attribute,
                    Source = source
                };
            }
            catch (Exception e) when (e != null)
            {
                // ignore exceptions
            }
            return default;
        }

        // TODO: implement description
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
                    Entity = new
                    {
                        attribute.Name,
                        Description = "Coming soon."
                    }
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
                    Entity = attribute
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
                    Entity = attribute
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
                    Entity = attribute
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
        #endregion

        #region *** Page Model ***
        /// <summary>
        /// Gets a RhinoPageModel object built from markdown table.
        /// </summary>
        /// <param name="pageModel">The PageModel to build by.</param>
        /// <param name="name">The PageModel name.</param>
        /// <param name="markdown">The markdown table.</param>
        /// <returns>RhinoPageModel object.</returns>
        public static RhinoPageModel GetFromMarkdown(this RhinoPageModel pageModel, string name, string markdown)
        {
            // parse
            var table = new DataTable().FromMarkDown(markdown);

            // setup
            pageModel.Name = name;

            // bad request
            if (table == default || table.Rows?.Count == 0)
            {
                return pageModel;
            }

            // build
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var entries = table
                .ToDictionary()
                .Select(i => JsonSerializer.Deserialize<RhinoPageModelEntry>(JsonSerializer.Serialize(i), options));

            // get
            return new RhinoPageModel
            {
                Name = pageModel.Name,
                Entries = entries
            };
        }
        #endregion
    }
}
