/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Abstraction.WebDriver;
using Gravity.Extensions;
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OpenQA.Selenium;

using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Interfaces;
using Rhino.Api.Parser;
using Rhino.Connectors.Text;
using Rhino.Controllers.Domain.Cache;
using Rhino.Controllers.Domain.Extensions;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;
using Rhino.Settings;

using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Rhino.Controllers.Domain.Data
{
    /// <summary>
    /// Data Access Layer for all static data.
    /// </summary>
    public class MetaDataRepository : IMetaDataRepository
    {
        // constants
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

        // members: state
        private readonly IRepository<RhinoModelCollection> _models;
        private readonly IRepository<RhinoConfiguration> _configurations;
        private readonly IEnumerable<Type> _types;
        private readonly AppSettings _appSettings;
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
            IRepository<RhinoConfiguration> configurations,
            IEnumerable<Type> types,
            AppSettings appSettings,
            ILogger logger)
        {
            // setup: fields
            _models = models;
            _configurations = configurations;
            _types = types;
            _appSettings = appSettings;
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

        #region *** Get Plugins ***
        /// <summary>
        /// Gets a list of non conditional actions with their literals default verbs
        /// </summary>
        /// <returns>List of non conditional actions</returns>
        public IEnumerable<ActionModel> GetPlugins()
        {
            return GetActionModels(_appSettings, Authentication);
        }

        /// <summary>
        /// Gets a list of non conditional actions with their literals default verbs
        /// </summary>
        /// <param name="configuration">Configuration ID.</param>
        /// <returns>List of non conditional actions</returns>
        public IEnumerable<ActionModel> GetPlugins(string configuration)
        {
            // setup
            var userPlugins = GetActionModels(_appSettings, Authentication);

            // external
            var (statusCode, configurationEntity) = _configurations.SetAuthentication(Authentication).Get(configuration);
            var isNullOrEmpty = string.IsNullOrEmpty(configuration);
            var isConfiguration = !isNullOrEmpty && statusCode == StatusCodes.Status200OK;
            var isExternal = isConfiguration && configurationEntity.ExternalRepositories?.Any() == true;

            // not found
            if (!isExternal)
            {
                _logger?.Debug($"Get-Actions -Configuration {configuration} = OK, {userPlugins.Count()}");
                return userPlugins;
            }

            // external
            foreach (var repository in configurationEntity.ExternalRepositories)
            {
                var repositoryName = string.IsNullOrEmpty(repository.Name) ? "external" : $"external:{repository.Name}";
                var cachedPlugins = MetaDataCache
                    .Plugins
                    .SelectMany(i => i.Value.ActionsCache)
                    .Where(i => i.Value.Source.Equals(repositoryName))
                    .Select(i => i.Value);

                if (cachedPlugins?.Any() == false)
                {
                    SyncExternalRepository(repository);
                    _logger.Debug($"Sync-ExternalRepository -Url {repository.Url} = OK");
                }

                userPlugins = userPlugins.Concat(cachedPlugins);
            }

            // get
            _logger?.Debug($"Get-Actions -Configuration {configuration} = OK, {userPlugins.Count()}");
            return userPlugins.Where(i => !string.IsNullOrEmpty(i.Key)).OrderBy(i => i.Key);
        }
        #endregion

        /// <summary>
        /// Gets a collection of available assertions (based on AssertMethodAttribute).
        /// </summary>
        /// <returns>A collection of AssertMethodAttribute.</returns>
        public IEnumerable<AssertModel> GetAssertions() => GetAssertions(_types);

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
                .Where(i => !string.IsNullOrEmpty(i.Key))
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

            // setup
            var driverFactory = _types.FirstOrDefault(t => t == typeof(DriverFactory));
            if (driverFactory == default)
            {
                return Array.Empty<DriverModel>();
            }

            // fetch attributes
            var attributes = driverFactory
                .GetMethods(Binding)
                .SelectMany(i => i.CustomAttributes)
                .Where(i => i.AttributeType.Name.Equals("DriverMethodAttribute", Compare))
                .SelectMany(i => i.NamedArguments.Where(i => i.MemberName.Equals("Driver", Compare)))
                .Select(i => new DriverModel { Key = $"{i.TypedValue.Value}", Literal = $"{i.TypedValue.Value}".ToSpaceCase().ToLower() })
                .Where(i => !string.IsNullOrEmpty(i.Key));

            // get
            return Enumerable.DistinctBy(attributes, (i) => i.Key).OrderBy(i => i.Key);
        }

        // TODO: implement GetExamples factory for getting examples for the different locators.
        /// <summary>
        /// Gets a list of all available locators.
        /// </summary>
        /// <returns>A list all available locators.</returns>
        public IEnumerable<BaseModel<object>> GetLocators()
        {
            return GetLocators(_types);
        }

        /// <summary>
        /// Gets a list of all available macros.
        /// </summary>
        /// <returns>List of all available macros.</returns>
        public IEnumerable<MacroModel> GetMacros()
        {
            return _types
                .GetMacroAttributes()
                .Select(i => ((MacroAttribute)i).ToModel())
                .Where(i => !string.IsNullOrEmpty(i.Key));
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
                .Where(i => !string.IsNullOrEmpty(i.Key))
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
            return serviceEvents.Select(GetModel).Where(i => !string.IsNullOrEmpty(i.Key));
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
            _logger?.Debug("Get-Version = OK");
            return await ControllerUtilities.ForceReadFileAsync(path: FileName).ConfigureAwait(false);
        }

        // TODO: replace with reflection on the next Rhino.Api version.
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
                ["test-parameters"] = ("A list of parameters the user can provide and are exposed by the plugin.", 9),
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

        // TODO: get by reflection from GravityApi when available.
        /// <summary>
        /// Gets a collection of all available RhinoVerbs.
        /// </summary>
        /// <returns>A collection of all available RhinoVerbs</returns>
        public IEnumerable<RhinoVerbModel> GetVerbs()
        {
            return new[]
            {
                new RhinoVerbModel{ Key = "on", Literal = "on", Entity = new { Scope = "onElement" } },
                new RhinoVerbModel{ Key = "to", Literal = "to", Entity = new { Scope = "onElement" } },
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
            return models.Concat(GetLocators(_types));
        }

        /// <summary>
        /// Gets an ASCII tree based on the RhinoTestCase spec provided.
        /// </summary>
        /// <param name="rhinoTestCase">The RhinoTestCase spec by which to create the ASCII tree.</param>
        /// <returns>An ASCII tree that represents the RhinoTestCase.</returns>
        public string GetTestTree(string rhinoTestCase)
        {
            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = new[] { rhinoTestCase },
                DriverParameters = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["driver"] = "ChromeDriver",
                        ["driverBinaries"] = "."
                    }
                }
            };
            var connector = new TextConnector(configuration, _types);
            var tree = new StringBuilder();
            var pattern = string.Join("|", GetAssertions().Select(i => i.Key.ToLower()).OrderBy(i => i.Length));

            // local
            void RenderTree(RhinoTestStep testStep, int level)
            {
                // normalize
                level = level < 1 ? 1 : level;

                // setup
                var isPlugin = testStep.Steps?.Any() == true;
                var actions = isPlugin ? testStep.Steps.ToArray() : Array.Empty<RhinoTestStep>();
                var actionLine = GetLine(level - 1);
                var entityType = actions.Length > 0 ? "(P)" : "(A)";
                var command = string.IsNullOrEmpty(testStep.Command) ? "MissingPlugin" : testStep.Command;

                // normalize
                entityType = command.Equals("MissingPlugin") ? "(E)" : entityType;
                command = command.ToSpaceCase().ToLower();

                // build
                var line = entityType.Equals("(E)")
                    ? $"{actionLine} {entityType} {command + " - " + Regex.Match(testStep.Action, "[^{]*").Value.Trim().ToLower()}"
                    : $"{actionLine} {entityType} {command}";

                // render entity
                tree.AppendLine(line);

                // collect models
                if (testStep.ModelEntries?.Any() == true)
                {
                    var modelsLine = GetLine(level);
                    var models = testStep.ModelEntries.ToArray();

                    for (int i = 0; i < models.Length; i++)
                    {
                        modelsLine = i == models.Length - 1 && !isPlugin
                            ? ReplaceLastOccurrence(modelsLine, "├──", "├──")
                            : modelsLine;
                        modelsLine = $"{modelsLine} (M) {models[i].Name}";
                        tree.AppendLine(modelsLine);
                    }
                }

                if (testStep.ExpectedResults?.Any() == true)
                {
                    var asserts = testStep.ExpectedResults.Select(i => Regex.Match(i.ExpectedResult, pattern).Value).ToArray();

                    for (int i = 0; i < asserts.Length; i++)
                    {
                        var assertLine = GetLine(level);
                        assertLine = i == asserts.Length - 1
                            ? ReplaceLastOccurrence(assertLine, "├──", "├──")
                            : assertLine;
                        assertLine = $"{assertLine} (R) assert {asserts[i]}";
                        tree.AppendLine(assertLine);
                    }
                }

                // root
                if (!isPlugin)
                {
                    return;
                }

                // setup tree level
                var treeLevel = level + 1;

                // iterate
                var nestedSteps = testStep.Steps.ToArray();
                for (int i = 0; i < nestedSteps.Length; i++)
                {
                    RenderTree(nestedSteps[i], treeLevel);
                }
            }

            // get tree line
            static string GetLine(int level, char seperator = '│')
            {
                // setup
                var levels = string.Empty;

                // build
                for (int i = 0; i < level; i++)
                {
                    levels += $"{seperator}   ";
                }

                // get
                return levels + "├──";
            }

            // replace the last occurrence on sub string
            static string ReplaceLastOccurrence(string source, string find, string replace)
            {
                var place = source.LastIndexOf(find);
                var result = source.Remove(place, find.Length).Insert(place, replace);
                return result;
            }

            // get
            var testCases = connector.ProviderManager.TestRun.TestCases.ToArray();

            // iterate
            foreach (var testCase in testCases)
            {
                tree.AppendLine(testCase.Key).AppendLine(".");
                foreach (var step in testCase.Steps)
                {
                    RenderTree(step, 1);
                }
                tree.AppendLine();
            }

            // get
            return $"{tree}";
        }

        /// <summary>
        /// Gets a collection of Gravity ActionRule based on the provided test specifications.
        /// </summary>
        /// <param name="rhinoTestCase">The test specifications.</param>
        /// <returns>A collection of Gravity ActionRule.</returns>
        public IEnumerable<ActionRule> GetGravityActions(string rhinoTestCase)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a collection of RhinoSymbolModel based to the provided test specifications.
        /// </summary>
        /// <param name="rhinoTestCase">The test specifications.</param>
        /// <returns>A collection of RhinoSymbolModel.</returns>
        public IEnumerable<RhinoSymbolModel> GetSymbols(string rhinoTestCase)
        {
            // setup
            var configuration = _configurations.GetOrDefault(id: string.Empty);
            configuration.TestsRepository = new[] { rhinoTestCase };

            // get
            return GetSymbols(rhinoTestCase, configuration);
        }

        /// <summary>
        /// Gets a collection of RhinoSymbolModel based to the provided test specifications and configuration.
        /// </summary>
        /// <param name="rhinoTestCase">The test specifications.</param>
        /// <param name="id">The configuration id.</param>
        /// <returns>A collection of RhinoSymbolModel.</returns>
        public IEnumerable<RhinoSymbolModel> GetSymbols(string rhinoTestCase, string id)
        {
            // setup
            var configuration = _configurations.GetOrDefault(id);
            configuration.TestsRepository = new[] { rhinoTestCase };

            // get
            return GetSymbols(rhinoTestCase, configuration);
        }

        private IEnumerable<RhinoSymbolModel> GetSymbols(string rhinoTestCase, RhinoConfiguration configuration)
        {
            // bad request
            if (string.IsNullOrEmpty(rhinoTestCase))
            {
                return Array.Empty<RhinoSymbolModel>();
            }

            // models
            var models = GetModels($"{configuration.Id}", _models, Authentication);
            configuration.Models = models.Select(i => JsonSerializer.Serialize(i, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

            // setup
            var connector = configuration.Resolve(_logger);
            var testsData = rhinoTestCase.CreateLines();

            // iterate
            var symbols = new List<RhinoSymbolModel>();
            foreach (var testCase in connector.ProviderManager.TestRun.TestCases)
            {
                var data = testsData.FirstOrDefault(i => i.Any(x => x.Text.Contains(testCase.Key, Compare)));
                var symbol = testCase.CreateSymbols(data);
                symbols.Add(symbol);
            }

            // get
            return symbols;
        }

        /// <summary>
        /// Gets a collection of ActionModel based to the provided test expression.
        /// </summary>
        /// <param name="model">The filter expressions.</param>
        /// <returns>A collection of ActionModel.</returns>
        public IEnumerable<FindPluginsResponseModel> FindPlugins(FindPluginsModel model)
        {
            // setup
            var actions = GetActionModels(_appSettings, Authentication).Select(i => i.Entity);
            var filterExpression = string.IsNullOrEmpty(model?.Expression) ? string.Empty : model.Expression;
            var actionsData = new DataTable().AddRows(actions).Filter(filterExpression);

            // get
            return actionsData.Rows.Cast<DataRow>().Select(i => i.ToModel());
        }

        // UTILITIES
        // execute GetActions routine
        private static IEnumerable<ActionModel> GetActionModels(AppSettings settings, Authentication authentication)
        {
            // setup
            var publicRepositories = new[] { "Rhino", "Gravity", "External" };
            var token = GetUserToken(authentication, settings);
            var publicPlugins = MetaDataCache
                .Plugins
                .Where(i => Array.Exists(publicRepositories, j => i.Key.Contains(j, StringComparison.OrdinalIgnoreCase)))
                .SelectMany(i => i.Value.ActionsCache)
                .Select(i => i.Value);

            var userPlugins = MetaDataCache.Plugins.TryGetValue(token, out var userPluginsOut) && !token.Equals("Rhino", Compare)
                ? userPluginsOut.ActionsCache.Select(i => i.Value)
                : Array.Empty<ActionModel>();

            // get 
            return publicPlugins
                .Concat(userPlugins)
                .Where(i => !string.IsNullOrEmpty(i.Key))
                .OrderBy(i => i.Key);
        }

        private static IEnumerable<RhinoPageModel> GetModels(
            string id,
            IRepository<RhinoModelCollection> models,
            Authentication authentication)
        {
            // setup
            return models
                .SetAuthentication(authentication)
                .Get()
                .Where(i => i.Configurations?.Any() == false || $"{i.Id}".Equals(id, Compare))
                .SelectMany(i => i.Models)
                .ToList();
        }

        private static IEnumerable<BaseModel<object>> GetLocators(IEnumerable<Type> types)
        {
            // get relevant by method
            var allMethods = types.SelectMany(t => t.GetMethods());
            var methods = new List<MemberInfo>();

            // TODO: remove this workaround when loading is complete
            // filter
            foreach (var method in allMethods)
            {
                try
                {
                    var isBy = method.IsStatic && method.ReturnType == typeof(By);
                    if (isBy)
                    {
                        methods.Add(method);
                    }
                }
                catch (Exception e) when (e != null)
                {
                    // ignore assembly
                }
            }

            // exit conditions
            if (!methods.Any())
            {
                return Array.Empty<LocatorModel>();
            }

            // invoke method
            return methods.Select(i => i.Name).Distinct().Select(i => new LocatorModel
            {
                Key = i,
                Literal = i.ToSpaceCase().ToLower(),
                Entity = new
                {
                    Name = i.ToSpaceCase().ToLower(),
                    Description = "Coming soon.",
                    Examples = Array.Empty<(string Description, string Example)>()
                },
                Verb = "using"
            });
        }

        private static IEnumerable<AssertModel> GetAssertions(IEnumerable<Type> types)
        {
            // constants
            const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            // setup
            return types
                .SelectMany(i => i.GetMethods(Flags))
                .Select(i => i.GetCustomAttribute<AssertMethodAttribute>())
                .Where(i => i != null)
                .Select(i => i.ToModel())
                .Where(i => i != default || !string.IsNullOrEmpty(i.Key))
                .OrderBy(i => i.Key);
        }

        private static string GetUserToken(Authentication authentication, AppSettings appSettings)
        {
            // setup conditions
            var isUser = !string.IsNullOrEmpty(authentication.Username);
            var isPassword = !string.IsNullOrEmpty(authentication.Password);

            // setup
            var path = Path.Combine(Environment.CurrentDirectory, RhinoPluginEntry.PluginsRhinoFolder);
            var encryptionKey = appSettings.StateManager?.DataEncryptionKey ?? string.Empty;
            var privateKey = "-" + JsonSerializer
                .Serialize(authentication)
                .ToBase64()
                .Encrypt(encryptionKey)
                .RemoveNonWord();
            var userPath = !isUser && !isPassword ? path : path + privateKey;

            // get
            return Path.GetFileName(userPath);
        }

        private static void SyncExternalRepository(ExternalRepository repository)
        {
            // collect actions
            var (name, actions) = repository.GetActions();
            var sourceName = string.IsNullOrEmpty(name) ? "external" : $"external:{name}";

            // bridge
            var pluginsCache = new ConcurrentDictionary<string, PluginCacheModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var action in actions)
            {
                var actionModel = action.ToModel(sourceName);
                var cacheModel = new PluginCacheModel
                {
                    ActionModel = actionModel,
                    Directory = repository.Url,
                    Path = $"/api/v{repository.Version}/gravity/actions/{actionModel.Entity.Name}",
                    Repository = repository
                };

                pluginsCache[actionModel.Entity.Name] = cacheModel;
            }

            // sync cache
            MetaDataCache.Plugins[sourceName] = new()
            {
                ActionsCache = pluginsCache.GetActionsCache(),
                PluginsCache = pluginsCache
            };
        }
    }
}
