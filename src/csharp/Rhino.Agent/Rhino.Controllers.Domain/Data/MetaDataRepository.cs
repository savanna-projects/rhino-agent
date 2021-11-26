/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Abstraction.WebDriver;
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;

using OpenQA.Selenium;

using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Interfaces;
using Rhino.Api.Parser;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;

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
        private readonly IPluginsRepository _plugins;
        private readonly IRepository<RhinoModelCollection> _models;
        private readonly IEnumerable<Type> _types;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of MetaDataRepository.
        /// </summary>
        /// <param name="plugins">An IPluginsRepository implementation.</param>
        /// <param name="types">An IEnumerable<Type> implementation.</param>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        public MetaDataRepository(
            IPluginsRepository plugins,
            IRepository<RhinoModelCollection> models,
            IEnumerable<Type> types,
            ILogger logger)
        {
            // setup: fields
            _plugins = plugins;
            _models = models;
            _types = types;
            _logger = logger?.CreateChildLogger(nameof(MetaDataRepository));

            // setup: properties
            Logger = _logger;
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
            foreach (var (source, entity) in InvokeGetActions())
            {
                var action = entity.ToModel(source);
                if (action != default)
                {
                    actions.Add(action);
                }
            }

            // get
            _logger?.Debug($"Get-Actions = Ok, {actions.Count}");
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
            return _types
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
            var onTypes = _types.Where(t => typeof(IConnector).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
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
            var driverFactory = _types.FirstOrDefault(t => t == typeof(DriverFactory));
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
        public IEnumerable<BaseModel<object>> GetLocators()
        {
            return InvokeGetLocators();
        }

        /// <summary>
        /// Gets a list of all available macros.
        /// </summary>
        /// <returns>List of all available macros.</returns>
        public IEnumerable<MacroModel> GetMacros()
        {
            return _types.GetMacroAttributes().Select(i => ((MacroAttribute)i).ToModel());
        }

        // TODO: implement GetExamples factory for getting examples for the different operators.
        /// <summary>
        /// Gets a list of all available operators.
        /// </summary>
        /// <returns>A list of all available operators.</returns>
        public IEnumerable<OperatorModel> GetOperators()
        {
            return new RhinoTestCaseFactory().OperatorsMap.Select(i => new OperatorModel
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
            return _types
                .Where(i => i.GetCustomAttribute<ReporterAttribute>() != null)
                .Select(i => i.GetCustomAttribute<ReporterAttribute>().ToModel())
                .OrderBy(i => i.Key);
        }

        /// <summary>
        /// Gets a list of all available reporters.
        /// </summary>
        /// <returns>A list all available reporters.</returns>
        public IEnumerable<ServiceEventModel> GetServiceEvents()
        {
            // constants
            const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static;

            // local
            static ServiceEventModel GetModel((string Event, ServiceEventAttribute Attribute) input) => new()
            {
                Entity = input.Attribute,
                Key = input.Event,
                Literal = input.Event.ToSpaceCase()
            };

            // build
            var fileds = _types.SelectMany(i => i.GetFields(Flags));
            var serviceEventFields = fileds.Where(i => i.GetCustomAttribute<ServiceEventAttribute>() != null);
            var serviceEvents = serviceEventFields
                .Select(i => ($"{i.GetValue(null)}", i.GetCustomAttribute<ServiceEventAttribute>()));

            // get
            return serviceEvents.Select(i => GetModel(input: i));
        }

        /// <summary>
        /// Gets a list of all available micro services under Rhino.Controllers.dll.
        /// </summary>
        /// <returns>A list all available reporters.</returns>
        public IEnumerable<string> GetServices()
        {
            // setup
            var types = Assembly.GetCallingAssembly().GetTypes();

            // build
            return types
                .Where(i => typeof(ControllerBase).IsAssignableFrom(i))
                .Select(i => i.Name.Replace("Controller", string.Empty).Trim().ToSpaceCase())
                .OrderBy(i => i);
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
                _logger?.Debug("Get-Version = NotFound");
                return string.Empty;
            }

            // get
            _logger?.Debug("Get-Version = Ok");
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
            var properties = new Dictionary<string, (string Description, int Priority)>
            {
                ["test-id"] = ("A _*unique*_ identifier of the test case. " + Exclude, 0),
                ["test-scenario"] = ("A statement describing the functionality to be tested. " + Exclude, 1),
                ["test-categories"] = ("A comma separated list of categories to which this test belongs to." + Exclude, 2),
                ["test-priority"] = ("The level of _*business importance*_ assigned to an item, e.g., defect." + Exclude, 3),
                ["test-severity"] = ("The degree of _*impact*_ that a defect has on the development or operation of a component or system." + Exclude, 4),
                ["test-tolerance"] = ("The % of the test tolerance. A Special attribute to decide, based on configuration if the test will be marked as passed or with warning. Default 0% tolerance. Must be a number with or without the % sign.", 5),
                ["test-actions"] = ("A collection of atomic pieces of logic which execute a single test case.", 6),
                ["test-data-provider"] = ("_*Data*_ created or selected to satisfy the execution preconditions and inputs to execute one or more _*test cases*_.", 7),
                ["test-expected-results"] = ("An ideal result that the tester should get after a test action is performed.", 8),
                ["test-parameters"] = ("A list of parameters the use can provide and are exposed by the plugin.", 9),
                ["test-examples"] = ("Mandatory! One or more examples of how to implement the Plugin in a test.", 10),
                ["test-models"] = ("A collection of elements and static data mapping which can be accessed by a reference for optimal reuse.", 11)
            };

            // get
            return properties.Select(i => new PropertyModel
            {
                Key = i.Key,
                Literal = "[" + i.Key.ToLower() + "]",
                Entity = new { Name = i.Key, i.Value.Description, i.Value.Priority },
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
            return _models
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => i.Configurations?.Any() == false);
        }

        /// <summary>
        /// Gets a collection of all available RhinoVerbs.
        /// </summary>
        /// <returns>A collection of all available RhinoVerbs</returns>
        public IEnumerable<RhinoVerbModel> GetVerbs()
        {
            return new[]
            {
                new RhinoVerbModel{ Key = "on", Literal = "on", Entity = new { Scope = "onElement" } },
                new RhinoVerbModel{ Key = "into", Literal = "into", Entity = new { Scope = "onElement" } },
                new RhinoVerbModel{ Key = "take", Literal = "take", Entity = new { Scope = "onElement" } },
                new RhinoVerbModel{ Key = "of", Literal = "of", Entity = new { Scope = "onElement" } },
                new RhinoVerbModel{ Key = "from", Literal = "from", Entity = new { Scope = "onAttribute" } },
                new RhinoVerbModel{ Key = "using", Literal = "using", Entity = new { Scope = "locatorType" } },
                new RhinoVerbModel{ Key = "by", Literal = "by", Entity = new { Scope = "locatorType" } },
                new RhinoVerbModel{ Key = "filter", Literal = "filter", Entity = new { Scope = "regularExpression" } },
                new RhinoVerbModel{ Key = "mask", Literal = "mask", Entity = new { Scope = "regularExpression" } },
                new RhinoVerbModel{ Key = "pattern", Literal = "pattern", Entity = new { Scope = "regularExpression" } }
            };
        }

        /// <summary>
        /// Gets a collection of all available model types.
        /// </summary>
        /// <returns>A collection of model types.</returns>
        public IEnumerable<BaseModel<object>> GetModelTypes()
        {
            // build
            var models = new RhinoModelTypeModel[]
            {
                new RhinoModelTypeModel
                {
                    Key = "Json",
                    Literal = "json",
                    Entity = new
                    {
                        Name = "json",
                        Description = "Save a serialized JSON object as a model.",
                        Examples = Array.Empty<(string Description, string Example)>()
                    },
                },
                new RhinoModelTypeModel
                {
                    Key = "Static",
                    Literal = "static",
                    Entity = new
                    {
                        Name = "static",
                        Description = "Save a static string as a model (use for static data models).",
                        Examples = Array.Empty<(string Description, string Example)>()
                    },
                }
            };

            // get
            return models.Concat(InvokeGetLocators());
        }

        // UTILITIES
        // execute GetActions routine
        private IEnumerable<(string Source, ActionAttribute Action)> InvokeGetActions()
        {
            // setup
            var gravityPlugins = _types.GetActionAttributes();
            var pluginSpecs = _plugins.SetAuthentication(Authentication).Get();
            var pluginObjcs = new RhinoPluginFactory().GetRhinoPlugins(pluginSpecs.ToArray());

            // convert
            var attributes = pluginObjcs.Select(i => (ActionModel.ActionSource.Plugin, i.ToAttribute()));
            _logger?.Debug($"Get-Actions -Source {ActionModel.ActionSource.Plugin} = Ok, {attributes.Count()}");

            // all actions
            // collect all potential types
            var actions = gravityPlugins
                .Select(i => (ActionModel.ActionSource.Code, (ActionAttribute)gravityPlugins.FirstOrDefault(a => a.Name == i.Name)))
                .Concat(attributes);
            actions = actions?.Any() == false ? Array.Empty<(string, ActionAttribute)>() : actions;
            _logger?.Debug($"Get-Actions -Source {ActionModel.ActionSource.Code} = Ok, {actions.Count()}");

            // get
            return actions;
        }

        private IEnumerable<BaseModel<object>> InvokeGetLocators()
        {
            // get relevant by method
            var methods = _types
                .SelectMany(t => t.GetMethods())
                .Where(m => m.IsStatic && m.ReturnType == typeof(By));

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
                    Description = "Coming soon.",
                    Examples = Array.Empty<(string Description, string Example)>()
                },
                Verb = "using"
            });
        }
    }
}
