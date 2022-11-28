/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Cli;
using Gravity.Abstraction.Logging;
using Gravity.Extensions;
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Interfaces;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;

using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for Rhino API related objects.
    /// </summary>
    public static partial class RhinoExtensions
    {
        #region *** Expressions ***
        [GeneratedRegex("{{\\$.*}}")]
        private static partial Regex GetArgumentToekn();
        #endregion

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
                    .Select(i => new KeyValuePair<string, object>(i.Parameter, i.Description))
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
                        Argument = GetArgumentToekn().Match(input: rhinoExample.Example).Value
                    }
                };
                onExamples.Add(onExample);
            }

            // get
            return onExamples;
        }

        private static ActionAttribute GetDefauleActionAttribute() => new()
        {
            CliArguments = new Dictionary<string, object>(),
            Description = string.Empty,
            Examples = Array.Empty<PluginExample>(),
            Name = string.Empty,
            Summary = string.Empty
        };

        #region *** Connector  ***
        /// <summary>
        /// Resolve all Rhino entities from the provided connector
        /// </summary>
        /// <param name="configuration">The configuration to resolve.</param>
        /// <returns>Resolved connector</returns>
        public static IConnector Resolve(this RhinoConfiguration configuration)
        {
            return Resolve(new TraceLogger("Rhino.Api", typeof(RhinoExtensions).Name), configuration);
        }

        /// <summary>
        /// Resolve all Rhino entities from the provided connector
        /// </summary>
        /// <param name="configuration">The configuration to resolve.</param>
        /// <param name="logger">Logger implementation to use with the connector.</param>
        /// <returns>Resolved connector</returns>
        public static IConnector Resolve(this RhinoConfiguration configuration, ILogger logger) => Resolve(logger, configuration);

        private static IConnector Resolve(ILogger logger, RhinoConfiguration configuration)
        {
            // setup
            var type = GetConnector(Utilities.Types, configuration);

            // get
            return (IConnector)Activator.CreateInstance(type, new object[] { configuration, Utilities.Types, logger });
        }
        /// <summary>
        /// Gets a connector.
        /// </summary>
        /// <param name="configuration">RhinoConfiguration by which to factor RhinoConnector</param>
        /// <returns>RhinoConnector implementation.</returns>
        public static Type GetConnector(this RhinoConfiguration configuration)
        {
            return GetConnector(Utilities.Types, configuration);
        }

        /// <summary>
        /// Gets a connector.
        /// </summary>
        /// <param name="configuration">RhinoConfiguration by which to factor RhinoConnector</param>
        /// <param name="types">A collection of <see cref="Type>"/> in which to search for RhinoConnector.</param>
        /// <returns>RhinoConnector implementation.</returns>
        public static Type GetConnector(this RhinoConfiguration configuration, IEnumerable<Type> types)
        {
            return GetConnector(types, configuration);
        }

        private static Type GetConnector(IEnumerable<Type> types, RhinoConfiguration configuration)
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
        /// Converts a DataRow object into ActionModel object.
        /// </summary>
        /// <param name="dataRow">DataRow to convert.</param>
        /// <returns>ActionModel object with ActionAttribute meta data.</returns>
        public static FindPluginsResponseModel ToModel(this DataRow dataRow)
        {
            return new()
            {
                Name = $"{dataRow["Name"]}",
                Summary = $"{dataRow["Summary"]}"
            };
        }

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
            var entries = table
                .ToDictionary()
                .Select(i => JsonSerializer.Deserialize<RhinoPageModelEntry>(JsonSerializer.Serialize(i)));

            // get
            return new RhinoPageModel
            {
                Name = pageModel.Name,
                Entries = entries
            };
        }
        #endregion

        #region *** Settings   ***
        /// <summary>
        /// Try to get the endpoint for Rhino Hub based on configuration and/or command-line arguments.
        /// </summary>
        /// <param name="appSettings">The configuration implementation to use.</param>
        /// <returns>Rhino Hub endpoint information.</returns>
        public static (string HubEndpoint, string HubAddress, string HubApiVersion) GetHubEndpoints(this AppSettings appSettings)
        {
            return GetHubEndpoints(cli: string.Empty, appSettings);
        }

        /// <summary>
        /// Try to get the endpoint for Rhino Hub based on configuration and/or command-line arguments.
        /// </summary>
        /// <param name="appSettings">The configuration implementation to use.</param>
        /// <param name="cli">The command line arguments to use.</param>
        /// <returns>Rhino Hub endpoint information.</returns>
        public static (string HubEndpoint, string HubAddress, string HubApiVersion) GetHubEndpoints(this AppSettings appSettings, string cli)
        {
            return GetHubEndpoints(cli, appSettings);
        }

        private static (string HubEndpoint, string HubAddress, string HubApiVersion) GetHubEndpoints(string cli, AppSettings appSettings)
        {
            // extract values
            var hubAddress = appSettings.Worker.HubAddress;
            var hubApiVersion = appSettings.Worker.HubApiVersion;

            // normalize
            hubAddress = string.IsNullOrEmpty(hubAddress) ? "http://localhost:9000" : hubAddress;
            hubApiVersion = string.IsNullOrEmpty(hubApiVersion) ? "1" : hubApiVersion;

            // get from command line
            if (string.IsNullOrEmpty(cli))
            {
                return ($"{hubAddress}/api/v{hubApiVersion}/rhino/orchestrator", hubAddress, hubApiVersion);
            }

            // parse
            var arguments = new CliFactory(cli).Parse();
            _ = arguments.TryGetValue("hubApiVersion", out string versionOut);
            var isHubVersion = int.TryParse(versionOut, out int hubVersionOut);

            hubAddress = arguments.TryGetValue("hubAddress", out string addressOut) ? addressOut : hubAddress;
            hubApiVersion = isHubVersion ? $"{hubVersionOut}" : hubApiVersion;

            // get
            return ($"{hubAddress}/api/v{hubApiVersion}/rhino/orchestrator", hubAddress, hubApiVersion);
        }

        /// <summary>
        /// Try to get the max parallel settings for Rhino worker based on configuration and/or command-line arguments.
        /// </summary>
        /// <param name="appSettings">The configuration implementation to use.</param>
        /// <returns>Rhino Hub endpoint information.</returns>
        public static int GetMaxParallel(this AppSettings appSettings)
        {
            return GetMaxParallel(cli: string.Empty, appSettings);
        }

        /// <summary>
        /// Try to get the max parallel settings for Rhino worker based on configuration and/or command-line arguments.
        /// </summary>
        /// <param name="appSettings">The configuration implementation to use.</param>
        /// <param name="cli">The command line arguments to use.</param>
        /// <returns>Rhino Hub endpoint information.</returns>
        public static int GetMaxParallel(this AppSettings appSettings, string cli)
        {
            return GetMaxParallel(cli, appSettings);
        }

        private static int GetMaxParallel(string cli, AppSettings appSettings)
        {
            // extract values
            var maxParallel = appSettings.Worker.MaxParallel;
            var arguments = new CliFactory(cli).Parse();

            // normalize
            maxParallel = maxParallel == default ? 1 : maxParallel;

            // get from command line
            if (!arguments.ContainsKey("maxParallel"))
            {
                return maxParallel;
            }

            // parse
            _ = arguments.TryGetValue("maxParallel", out string maxParallelValue);
            var isMaxParallel = int.TryParse(maxParallelValue, out int maxParallelout);

            maxParallel = isMaxParallel ? maxParallelout : maxParallel;

            // get
            return maxParallel;
        }
        #endregion
    }
}
