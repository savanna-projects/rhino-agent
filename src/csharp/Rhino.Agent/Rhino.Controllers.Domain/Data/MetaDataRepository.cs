/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Abstraction.WebDriver;
using Gravity.Extensions;
using Gravity.Services.Comet;
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

using OpenQA.Selenium;

using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Interfaces;
using Rhino.Api.Parser;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rhino.Controllers.Domain.Data
{
    /// <summary>
    /// Data Access Layer for all static data.
    /// </summary>
    public class MetaDataRepository : IMetaDataRepository
    {
        // members: state
        private readonly Orbit client;
        private readonly IPluginsRepository plugins;
        private readonly IRepository<RhinoModelCollection> models;
        private readonly IEnumerable<Type> types;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of StaticDataRepository.
        /// </summary>
        /// <param name="client">An Orbit client to fetch data from Gravity components.</param>
        /// <param name="plugins">An IPluginsRepository implementation.</param>
        /// <param name="types">An IEnumerable<Type> implementation.</param>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        public MetaDataRepository(
            Orbit client,
            IPluginsRepository plugins,
            IRepository<RhinoModelCollection> models,
            IEnumerable<Type> types,
            ILogger logger)
        {
            this.client = client;
            this.plugins = plugins;
            this.models = models;
            Logger = logger;
            this.types = types;
            this.logger = logger?.CreateChildLogger(nameof(MetaDataRepository));
        }

        /// <summary>
        /// Gets the Authentication object used by the repository.
        /// </summary>
        public Authentication Authentication { get; private set; }

        /// <summary>
        /// Gets or sets the logger implementation used by the repository.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets a list of non conditional actions with their literals default verbs
        /// </summary>
        /// <returns>List of non conditional actions</returns>
        public IEnumerable<ActionModel> GetPlugins()
        {
            // setup
            var actions = new List<ActionModel>();

            // build
            foreach (var (source, entity) in DoGetActions())
            {
                var action = entity.ToModel(source);
                if (action != default)
                {
                    actions.Add(action);
                }
            }

            // get
            logger?.Debug($"Get-Actions = Ok, {actions.Count}");
            return actions.OrderBy(i => i.Key);
        }

        /// <summary>
        /// Gets a collection of available assertions (based on AssertMethodAttribute).
        /// </summary>
        /// <returns>A collection of AssertMethodAttribute.</returns>
        public IEnumerable<AssertModel> GetAssertions()
        {
            // constants
            const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            // setup
            return types
                .SelectMany(i => i.GetMethods(Flags))
                .Select(i => i.GetCustomAttribute<AssertMethodAttribute>())
                .Where(i => i != null)
                .Select(i => i.ToModel())
                .Where(i => i != default)
                .OrderBy(i => i.Key);
        }

        /// <summary>
        /// Gets a list of all available connectors.
        /// </summary>
        /// <returns>A list all available connectors.</returns>
        public IEnumerable<ConnectorModel> GetConnectors()
        {
            // setup
            var onTypes = types.Where(t => typeof(IConnector).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
            var connectorTypes = onTypes.Where(i => i.GetCustomAttribute<ConnectorAttribute>() != null);

            // get
            return connectorTypes
                .Select(i => i.GetCustomAttribute<ConnectorAttribute>().ToModel())
                .OrderBy(i => i.Key);
        }

        // TODO: change the conversion type to ToModel when DriverMethodAttibute is available on Gravity.Abstraction release
        /// <summary>
        /// Gets a list of all available drivers.
        /// </summary>
        /// <returns>A list all available drivers.</returns>
        public IEnumerable<DriverModel> GetDrivers()
        {
            // constants
            const BindingFlags Binding = BindingFlags.Instance | BindingFlags.NonPublic;
            const StringComparison Compare = StringComparison.Ordinal;

            // setup
            var driverFactory = types.FirstOrDefault(t => t == typeof(DriverFactory));
            if (driverFactory == default)
            {
                return Array.Empty<DriverModel>();
            }

            // fetch attributes
            return driverFactory
                .GetMethods(Binding)
                .SelectMany(i => i.CustomAttributes)
                .Where(i => i.AttributeType.Name.Equals("DriverMethodAttribute", Compare))
                .SelectMany(i => i.NamedArguments.Where(i => i.MemberName.Equals("Driver", Compare)))
                .Select(i => new DriverModel { Key = $"{i.TypedValue.Value}", Literal = $"{i.TypedValue.Value}".ToSpaceCase().ToLower() })
                .DistinctBy(i => i.Key)
                .OrderBy(i => i.Key);
        }

        // TODO: implement GetExamples factory for getting examples for the different locators.
        /// <summary>
        /// Gets a list of all available locators.
        /// </summary>
        /// <returns>A list all available locators.</returns>
        public IEnumerable<LocatorModel> GetLocators()
        {
            // get relevant by method
            var methods = types.SelectMany(t => t.GetMethods()).Where(m => m.IsStatic && m.ReturnType == typeof(By));

            // exit conditions
            if (!methods.Any())
            {
                return Array.Empty<LocatorModel>();
            }

            // invoke method
            return methods.Select(i => i.Name).Distinct().Select(i => new LocatorModel
            {
                Key = i,
                Literal = Api.Extensions.StringExtensions.ToSpaceCase(i).ToLower(),
                Entity = new
                {
                    Name = Api.Extensions.StringExtensions.ToSpaceCase(i).ToLower(),
                    Description = "",
                    Examples = Array.Empty<(string Description, string Example)>()
                },
                Verb = "using"
            });
        }

        /// <summary>
        /// Gets a list of all available macros.
        /// </summary>
        /// <returns>List of all available macros.</returns>
        public IEnumerable<MacroModel> GetMacros()
        {
            return types.GetMacroAttributes().Select(i => ((MacroAttribute)i).ToModel());
        }

        // TODO: implement GetExamples factory for getting examples for the different operators.
        /// <summary>
        /// Gets a list of all available operators.
        /// </summary>
        /// <returns>A list of all available operators.</returns>
        public IEnumerable<OperatorModel> GetOperators()
        {
            return new RhinoTestCaseFactory(client).OperatorsMap.Select(i => new OperatorModel
            {
                Key = i.Key,
                Literal = i.Value.ToLower(),
                Entity = new { Examples = Array.Empty<(string Description, string Example)>() }
            });
        }

        /// <summary>
        /// Gets a list of all available reporters.
        /// </summary>
        /// <returns>A list all available reporters.</returns>
        public IEnumerable<ReporterModel> GetReporters()
        {
            return types
                .Where(i => i.GetCustomAttribute<ReporterAttribute>() != null)
                .Select(i => i.GetCustomAttribute<ReporterAttribute>().ToModel())
                .OrderBy(i => i.Key);
        }

        /// <summary>
        /// Sets the Authentication object which will be used by the repository.
        /// </summary>
        /// <param name="authentication">The Authentication object.</param>
        /// <returns>Self reference.</returns>
        public IMetaDataRepository SetAuthentication(Authentication authentication)
        {
            // setup
            Authentication = authentication;

            // get
            return this;
        }

        /// <summary>
        /// Gets the version the Rhino Server instance (if available).
        /// </summary>
        /// <returns>Rhino Server version.</returns>
        public async Task<string> GetVersionAsync()
        {
            // constants
            const string FileName = "version.txt";

            // not found
            if (!File.Exists(FileName))
            {
                logger?.Debug("Get-Version = NotFound");
                return string.Empty;
            }

            // get
            logger?.Debug("Get-Version = Ok");
            return await ControllerUtilities.ForceReadFileAsync(path: FileName).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a list of all available properties.
        /// </summary>
        /// <returns>A list all available properties.</returns>
        public IEnumerable<PropertyModel> GetAnnotations()
        {
            // constants
            const string Exclude = "Must not include spaces, escape or special characters - excluding dash and underscore.";

            // build
            var properties = new Dictionary<string, string>
            {
                ["test-id"] = "A _*unique*_ identifier of the test case. " + Exclude,
                ["test-scenario"] = "A statement describing the functionality to be tested. " + Exclude,
                ["test-priority"] = "The level of _*business importance*_ assigned to an item, e.g., defect." + Exclude,
                ["test-severity"] = "The degree of _*impact*_ that a defect has on the development or operation of a component or system." + Exclude,
                ["test-tolerance"] = "The % of the test tolerance. A Special attribute to decide, based on configuration if the test will be marked as passed or with warning. Default 0% tolerance. Must be a number with or without the % sign.",
                ["test-actions"] = "A collection of atomic pieces of logic which execute a single test case.",
                ["test-expected-results"] = "An ideal result that the tester should get after a test action is performed.",
                ["test-data-provider"] = "_*Data*_ created or selected to satisfy the execution preconditions and inputs to execute one or more _*test cases*_."
            };

            // get
            return properties.Select(i => new PropertyModel
            {
                Key = i.Key,
                Literal = "[" + i.Key.ToLower() + "]",
                Entity = new { Name = i.Key, Description = i.Value },
                Verb = string.Empty
            });
        }

        /// <summary>
        /// Gets a collection of all available RhinoModelCollection which are not
        /// connected to a specific configuration.
        /// </summary>
        /// <returns>A collection of RhinoModelCollection</returns>
        public IEnumerable<RhinoModelCollection> GetModels()
        {
            return models
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => i.Configurations?.Any() == false);
        }

        // UTILITIES
        // execute GetActions routine
        private IEnumerable<(string Source, ActionAttribute Action)> DoGetActions()
        {
            // setup
            var gravityPlugins = types.GetActionAttributes();
            var pluginSpecs = plugins.SetAuthentication(Authentication).Get();
            var pluginObjcs = new RhinoPluginFactory().GetRhinoPlugins(pluginSpecs.ToArray());

            // convert
            var attributes = pluginObjcs.Select(i => (ActionModel.ActionSource.Plugin, i.ToAttribute()));
            logger?.Debug($"Get-Actions -Source {ActionModel.ActionSource.Plugin} = Ok, {attributes.Count()}");

            // all actions
            // collect all potential types
            var actions = gravityPlugins
                .Select(i => (ActionModel.ActionSource.Code, (ActionAttribute)gravityPlugins.FirstOrDefault(a => a.Name == i.Name)))
                .Concat(attributes);
            actions = actions?.Any() == false ? Array.Empty<(string, ActionAttribute)>() : actions;
            logger?.Debug($"Get-Actions -Source {ActionModel.ActionSource.Code} = Ok, {actions.Count()}");

            // get
            return actions;
        }
    }
}