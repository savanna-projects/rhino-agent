/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Services.DataContracts;

using LiteDB;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Extensions;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;
using Rhino.Settings;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Rhino.Controllers.Domain.Automation
{
    /// <summary>
    /// Data Access Layer for Rhino API and integration operations.
    /// </summary>
    public partial class RhinoRepository : IRhinoRepository, IRhinoAsyncRepository
    {
        // constants
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

        #region *** Expressions ***
        [GeneratedRegex(@"^\w{8}-(\w{4}-){3}\w{12}$")]
        private static partial Regex GetIdPattern();

        [GeneratedRegex(@"^([a-z,A-Z]:\\|\/)\\w+")]
        private static partial Regex GetAbsolutePathPattern();
        #endregion

        // members: cache
        private readonly static IDictionary<string, AsyncStatusModel<RhinoConfiguration>> s_status
            = new ConcurrentDictionary<string, AsyncStatusModel<RhinoConfiguration>>();

        // members: state
        private readonly IEnumerable<Type> _types;
        private readonly IRepository<RhinoConfiguration> _configurationsRepository;
        private readonly IRepository<RhinoModelCollection> _modelsRespository;
        private readonly ITestsRepository _testsRepository;
        private readonly ILogger _logger;

        [Obsolete("This field is obsolete and will be removed in future versions. Please use `_appSettings` instead.")]
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings; // TODO: replace configuration

        /// <summary>
        /// Creates a new instance of StaticDataRepository.
        /// </summary>
        /// <param name="types">An IEnumerable<Type> implementation.</param>
        /// <param name="configurationsRepository">An IRepository<RhinoConfiguration> implementation to use with the Repository.</param>
        /// <param name="modelsRespository">An IRepository<RhinoModelCollection> implementation to use with the Repository.</param>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        public RhinoRepository(
            IEnumerable<Type> types,
            IRepository<RhinoConfiguration> configurationsRepository,
            IRepository<RhinoModelCollection> modelsRespository,
            ITestsRepository testsRepository,
            ILogger logger,
            IConfiguration appSettings)
        {
            _types = types;
            _configurationsRepository = configurationsRepository;
            _modelsRespository = modelsRespository;
            _testsRepository = testsRepository;
            _logger = logger.CreateChildLogger(nameof(RhinoRepository));
            _configuration = appSettings;
            _appSettings = new AppSettings(_configuration);
        }

        /// <summary>
        /// Gets the Authentication object used by the repository.
        /// </summary>
        public Authentication Authentication { get; private set; }

        #region *** Invoke         ***
        /// <summary>
        /// Invokes (run) a RhinoConfiguration.
        /// </summary>
        /// <param name="configuration">The RhinoConfiguration to invoke.</param>
        /// <returns>Status code and RhinoTestRun object (if any).</returns>
        public GenericResultModel<RhinoTestRun> InvokeConfiguration(RhinoConfiguration configuration)
        {
            // setup
            var configurationResult = SetConfiguration(configuration);

            // failed
            if (configurationResult.StatusCode != StatusCodes.Status200OK)
            {
                return new GenericResultModel<RhinoTestRun>
                {
                    Entity = default,
                    Message = configurationResult.Message,
                    StatusCode = configurationResult.StatusCode,
                };
            }

            // get
            return Invoke(configurations: new[] { configurationResult.Entity }).FirstOrDefault();
        }

        /// <summary>
        /// Invokes (run) a RhinoConfiguration.
        /// </summary>
        /// <param name="configuration">The RhinoConfiguration.Id to invoke.</param>
        /// <returns>Status code and RhinoTestRun object (if any).</returns>
        public GenericResultModel<RhinoTestRun> InvokeConfiguration(string configuration)
        {
            // setup
            var (statusCode, entity) = _configurationsRepository
                .SetAuthentication(Authentication)
                .Get(configuration);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                var notFound = $"Invoke-Configuration -Id {configuration} = (NotFound | NoConfiguration)";
                _logger?.Fatal(notFound);
                return new GenericResultModel<RhinoTestRun>
                {
                    Entity = default,
                    Message = notFound,
                    StatusCode = statusCode
                };
            }

            // build
            entity.Id = $"{entity.Id}".Equals(configuration, Compare)
                ? entity.Id
                : Guid.Parse(configuration);
            var configurationResult = SetConfiguration(entity);

            // error
            if (configurationResult.StatusCode != StatusCodes.Status200OK)
            {
                return new GenericResultModel<RhinoTestRun>
                {
                    Entity = default,
                    Message = configurationResult.Message,
                    StatusCode = configurationResult.StatusCode
                };
            }

            // get
            return Invoke(configurations: new[] { configurationResult.Entity }).FirstOrDefault();
        }

        /// <summary>
        /// Invokes (run) a RhinoConfiguration.
        /// </summary>
        /// <param name="configuration">The RhinoConfiguration.Id to execute configuration by.</param>
        /// <param name="spec">The Rhino spec to execute.</param>
        /// <returns>A RhinoTestRun object.</returns>
        public RhinoTestRun InvokeConfiguration(string configuration, string spec)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invokes (run) a RhinoConfiguration.
        /// </summary>
        /// <param name="driverParams">Explicit driver parameters to run by.</param>
        /// <param name="spec">The Rhino spec to execute.</param>
        /// <returns>A RhinoTestRun object.</returns>
        /// <remarks>This method will create a default RhinoConfiguration.</remarks>
        public RhinoTestRun InvokeConfiguration(IDictionary<string, object> driverParams, string spec)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invokes (run) a collection of RhinoConfiguration.
        /// </summary>
        /// <param name="collection">The RhinoTestsCollection to run configurations by.</param>
        /// <param name="isParallel"><see cref="true"/> to run all configurations in parallel.</param>
        /// <param name="maxParallel">The maximum number of configurations to run in parallel, will be ignored if isParallel argument is <see cref="false"/>.</param>
        /// <returns>A collection of status code and RhinoTestRun object (if any).</returns>
        public IEnumerable<GenericResultModel<RhinoTestRun>> InvokeCollection(string collection, bool isParallel, int maxParallel)
        {
            // setup
            var (statusCode, entity) = _testsRepository.SetAuthentication(Authentication).Get(collection);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                var notFound = $"Invoke-Collection -Id {collection} = (NotFound | NoCollection)";
                _logger?.Fatal(notFound);
                return new[]
                {
                    new GenericResultModel<RhinoTestRun>
                    {
                        Entity = default,
                        Message = notFound,
                        StatusCode = statusCode
                    }
                };
            }
            if (entity.Configurations.Count == 0)
            {
                var notFound = $"Invoke-Collection -Id {collection} = (NotFound | NoConfigurations)";
                _logger?.Fatal(notFound);
                return new[]
                {
                    new GenericResultModel<RhinoTestRun>
                    {
                        Entity = default,
                        Message = notFound,
                        StatusCode = statusCode
                    }
                };
            }

            // build
            var sourceConfigurations = entity.Configurations.Select(i => i.ToUpper()).ToArray();
            var configurations = _configurationsRepository
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => sourceConfigurations.Contains($"{i.Id}".ToUpper()))
                .Select(SetConfiguration)
                .Where(i => i.StatusCode == StatusCodes.Status200OK)
                .Select(i => i.Entity);

            // get
            return Invoke(configurations, isParallel, maxParallel);
        }
        #endregion

        #region *** Invoke Async   ***
        /// <summary>
        /// Invokes (run) a RhinoConfiguration.
        /// </summary>
        /// <param name="configuration">The RhinoConfiguration to invoke.</param>
        /// <returns>Status code and RhinoTestRun object (if any).</returns>
        public AsyncInvokeModel StartConfiguration(RhinoConfiguration configuration)
        {
            // setup
            var configurationResult = SetConfiguration(configuration);

            // error
            if (configurationResult.StatusCode != StatusCodes.Status200OK)
            {
                return new AsyncInvokeModel
                {
                    Id = Guid.Empty, StatusCode =
                    configurationResult.StatusCode,
                    StatusEndpoint = default
                };
            }

            // build
            var (action, model) = Start(configurationResult.Entity);

            // invoke
            action.Start();

            // get
            return model;
        }

        /// <summary>
        /// Invokes (run) a RhinoConfiguration.
        /// </summary>
        /// <param name="configuration">The RhinoConfiguration.Id to invoke.</param>
        /// <returns>Status code and RhinoTestRun object (if any).</returns>
        public AsyncInvokeModel StartConfiguration(string configuration)
        {
            // setup
            var (statusCode, entity) = _configurationsRepository.SetAuthentication(Authentication).Get(configuration);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return new AsyncInvokeModel { Id = Guid.Empty, StatusCode = StatusCodes.Status404NotFound, StatusEndpoint = default };
            }

            // build
            entity.Id = $"{entity.Id}".Equals(configuration, Compare) ? entity.Id : Guid.Parse(configuration);
            var configurationResult = SetConfiguration(entity);

            // error
            if (configurationResult.StatusCode != StatusCodes.Status200OK)
            {
                return new AsyncInvokeModel
                {
                    Id = Guid.Empty,
                    StatusCode = configurationResult.StatusCode,
                    StatusEndpoint = default
                };
            }

            // build
            var (action, model) = Start(configurationResult.Entity);

            // invoke
            action.Start();

            // get
            return model;
        }

        /// <summary>
        /// Invokes (run) a collection of RhinoConfiguration.
        /// </summary>
        /// <param name="collection">The RhinoTestsCollection to run configurations by.</param>
        /// <param name="isParallel"><see cref="true"/> to run all configurations in parallel.</param>
        /// <param name="maxParallel">The maximum number of configurations to run in parallel, will be ignored if isParallel argument is <see cref="false"/>.</param>
        /// <returns>A collection of status code and RhinoTestRun object (if any).</returns>
        public IEnumerable<AsyncInvokeModel> StartCollection(string collection, bool isParallel, int maxParallel)
        {
            // setup
            maxParallel = isParallel ? maxParallel : 1;
            maxParallel = maxParallel < 1 ? Environment.ProcessorCount : maxParallel;

            var options = new ParallelOptions { MaxDegreeOfParallelism = maxParallel };
            var (statusCode, entity) = _testsRepository.SetAuthentication(Authentication).Get(collection);

            // not found
            var notFound = new AsyncInvokeModel
            {
                Id = Guid.Empty,
                StatusCode = StatusCodes.Status404NotFound,
                StatusEndpoint = default
            };
            if (statusCode == StatusCodes.Status404NotFound)
            {
                _logger?.Warn($"Start-Collection -Id {collection} = (NotFound, Collection)");
                return new[] { notFound };
            }
            if (entity.Configurations.Count == 0)
            {
                _logger?.Warn($"Start-Collection -Id {collection} = (NotFound, Configurations)");
                return new[] { notFound };
            }

            // build
            var sourceConfigurations = entity.Configurations.Select(i => i.ToUpper()).ToArray();
            var configurations = _configurationsRepository
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => sourceConfigurations.Contains($"{i.Id}".ToUpper()))
                .Select(SetConfiguration)
                .Where(i => i.StatusCode == StatusCodes.Status200OK)
                .Select(i => Start(i.Entity))
                .ToArray();

            // invoke
            Task.Run(() => Parallel.ForEach(configurations.Select(i => i.Action), options, action => action.RunSynchronously()));

            // get
            return configurations.Select(i => i.Model);
        }

        private (Task Action, AsyncInvokeModel Model) Start(RhinoConfiguration configuration)
        {
            // setup
            var id = Guid.NewGuid();
            var endpoint = $"/api/v3/rhino/async/status/{id}";

            // register
            s_status[$"{id}"] = new AsyncStatusModel<RhinoConfiguration>
            {
                EntityOut = default,
                Progress = default,
                Status = AsyncStatus.Pending,
                RuntimeId = $"{id}"
            };

            // delegate
            var action = new Task(() =>
            {
                s_status[$"{id}"].Status = AsyncStatus.Running;
                var runResult = Invoke(new[] { configuration }).First();

                s_status[$"{id}"] = new AsyncStatusModel<RhinoConfiguration>
                {
                    EntityOut = runResult.Entity,
                    Progress = 100,
                    Status = runResult.StatusCode != StatusCodes.Status200OK ? AsyncStatus.Failed : AsyncStatus.Complete,
                    StatusCode = runResult.StatusCode,
                    EntityIn = configuration,
                    RuntimeId = $"{id}"
                };
            });

            // get
            return (action, new AsyncInvokeModel { Id = id, StatusCode = 201, StatusEndpoint = endpoint });
        }
        #endregion

        #region *** Authentication ***
        /// <summary>
        /// Sets the Authentication object which will be used by the repository.
        /// </summary>
        /// <param name="authentication">The Authentication object.</param>
        /// <returns>Self reference.</returns>
        public IRhinoRepository SetAuthentication(Authentication authentication)
        {
            // setup
            Authentication = authentication;

            // get
            return this;
        }

        /// <summary>
        /// Sets the Authentication object which will be used by the repository.
        /// </summary>
        /// <param name="authentication">The Authentication object.</param>
        /// <returns>Self reference.</returns>
        IRhinoAsyncRepository IHasAuthentication<IRhinoAsyncRepository>.SetAuthentication(Authentication authentication)
        {
            // setup
            Authentication = authentication;

            // get
            return this;
        }
        #endregion

        #region *** Get            ***
        /// <summary>
        /// Gets all async invoke status from the application cache.
        /// </summary>
        /// <returns>A collection of AsyncStatusModel object (if any).</returns>
        public IEnumerable<AsyncStatusModel<RhinoConfiguration>> GetStatus()
        {
            // find
            var statusCollection =
                (ConcurrentDictionary<string, AsyncStatusModel<RhinoConfiguration>>)s_status;
            var completed = new List<AsyncStatusModel<RhinoConfiguration>>();

            // clean
            foreach (var status in statusCollection)
            {
                var isPending = status.Value.Status == AsyncStatus.Pending;
                var isRuning = isPending || status.Value.Status == AsyncStatus.Running;
                if (isRuning)
                {
                    continue;
                }

                statusCollection.TryRemove(status.Key, out AsyncStatusModel<RhinoConfiguration> value);
                completed.Add(value);
            }

            // get
            return s_status.Select(i => i.Value).Concat(completed).OrderBy(i => i.Status);
        }

        /// <summary>
        /// Gets an async invoke status from the application cache.
        /// </summary>
        /// <param name="id">The invoke ID by which to get status.</param>
        /// <returns>Status code and AsyncStatusModel object (if any).</returns>
        public (int StatusCode, AsyncStatusModel<RhinoConfiguration> Status) GetStatus(Guid id)
        {
            // not found
            if (!s_status.ContainsKey($"{id}"))
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // setup
            var statusCollection =
                (ConcurrentDictionary<string, AsyncStatusModel<RhinoConfiguration>>)s_status;
            var statusModel = statusCollection[$"{id}"];

            // OK
            var isPending = statusModel.Status == AsyncStatus.Pending;
            var isRuning = isPending || statusModel.Status == AsyncStatus.Running;
            if (isRuning)
            {
                return (StatusCodes.Status200OK, statusModel);
            }

            // clear
            statusCollection.TryRemove($"{id}", out AsyncStatusModel<RhinoConfiguration> value);

            // get
            return (StatusCodes.Status200OK, value);
        }
        #endregion

        #region *** Delete         ***
        /// <summary>
        /// Deletes an invoke status from the domain state.
        /// </summary>
        /// <param name="id">The invoke status id by which to delete.</param>
        /// <returns><see cref="int"/>.</returns>
        public int Delete(string id)
        {
            // delete
            s_status.Remove(id);

            // get
            return StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Deletes all invoke status from the domain state.
        /// </summary>
        /// <returns><see cref="int"/>.</returns>
        public int Delete()
        {
            // delete
            s_status.Clear();

            // get
            return StatusCodes.Status204NoContent;
        }
        #endregion

        // Invocation
        private IEnumerable<GenericResultModel<RhinoTestRun>> Invoke(
            IEnumerable<RhinoConfiguration> configurations,
            bool isParallel = false,
            int maxParallel = 0)
        {
            // setup
            maxParallel = isParallel ? maxParallel : 1;
            maxParallel = maxParallel < 1 ? Environment.ProcessorCount : maxParallel;
            var options = new ParallelOptions { MaxDegreeOfParallelism = maxParallel };
            var results = new ConcurrentBag<GenericResultModel<RhinoTestRun>>();

            // invoke
            Parallel.ForEach(configurations, options, configuration =>
            {
                try
                {
                    configuration.ReportConfiguration.ReportOut = GetReportsOutLocation(configuration);
                    configuration.ReportConfiguration.LogsOut = GetLogsOutLocation(configuration);
                    var responseBody = configuration.Invoke(_types);
                    var ok = $"Invoke-Configuration = {responseBody.Key}";
                    _logger?.Debug(ok);
                    results.Add(new GenericResultModel<RhinoTestRun>
                    {
                        Entity = responseBody,
                        StatusCode = StatusCodes.Status200OK,
                        Message = ok
                    });
                }
                catch (Exception e) when (e != null)
                {
                    var fatal = $"Invoke-Configuration = (InternalServerError | {e.GetBaseException().Message})";
                    _logger?.Error(fatal);
                    results.Add(new GenericResultModel<RhinoTestRun>
                    {
                        Entity = default,
                        Message = fatal,
                        StatusCode = StatusCodes.Status500InternalServerError
                    });
                }
            });

            // get
            return results;
        }

        private static string GetReportsOutLocation(RhinoConfiguration configuration)
        {
            // setup
            var reportsOut = configuration.ReportConfiguration.ReportOut;

            // bad request
            if (string.IsNullOrEmpty(reportsOut) || reportsOut.Equals("."))
            {
                return Path.Combine(Environment.CurrentDirectory, "Outputs", "Reports", "Rhino");
            }

            // get
            return Path.IsPathRooted(reportsOut)
                ? reportsOut
                : Path.Combine(Environment.CurrentDirectory, reportsOut);
        }

        private static string GetLogsOutLocation(RhinoConfiguration configuration)
        {
            // setup
            var logsOut = configuration.ReportConfiguration.LogsOut;

            // bad request
            if (string.IsNullOrEmpty(logsOut) || logsOut.Equals("."))
            {
                return Path.Combine(Environment.CurrentDirectory, "Outputs", "Logs");
            }

            // get
            return Path.IsPathRooted(logsOut)
                ? logsOut
                : Path.Combine(Environment.CurrentDirectory, logsOut);
        }

        // Configuration Setup Pipeline
        private GenericResultModel<RhinoConfiguration> SetConfiguration(RhinoConfiguration configuration)
        {
            // validation
            var statusCode = AssertConfiguration(configuration);
            if (statusCode != -1)
            {
                return new GenericResultModel<RhinoConfiguration>()
                {
                    Entity = configuration,
                    Message = $"Assert-Configuration = (Fail | {statusCode})",
                    StatusCode = statusCode
                };
            }

            //  setup conditions
            var isUser = Authentication.Username != string.Empty;

            // build
            configuration.Authentication = isUser ? Authentication : configuration.Authentication;

            // build
            SetModels(configuration);
            SetTestsRepository(configuration);
            SetSettings(configuration);

            // invoke
            try
            {
                _logger?.Debug("Create-Configuration = OK");
                return new GenericResultModel<RhinoConfiguration>()
                {
                    Entity = configuration,
                    Message = "Create-Configuration = OK",
                    StatusCode = statusCode
                };
            }
            catch (Exception e) when (e != null)
            {
                var fatal = e.GetBaseException().Message;
                _logger?.Fatal($"Create-Configuration = (InternalServerError, ({fatal})", e);
                return new GenericResultModel<RhinoConfiguration>()
                {
                    Entity = configuration,
                    Message = $"Create-Configuration = (InternalServerError, ({fatal})",
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        private int AssertConfiguration(RhinoConfiguration configuration)
        {
            // bad request            
            if (!configuration.TestsRepository.Any())
            {
                _logger?.Debug("Invoke-Configuration = (BadRequest, NoTests)");
                return StatusCodes.Status400BadRequest;
            }
            if (!configuration.DriverParameters.Any())
            {
                _logger?.Debug("Invoke-Configuration = (BadRequest, NoDrivers)");
                return StatusCodes.Status400BadRequest;
            }

            // get
            return StatusCodes.Status200OK;
        }

        private void SetModels(RhinoConfiguration configuration)
        {
            // setup
            var userModels = configuration
                .Models
                .Select(i => GetIdPattern().Match(i).Value)
                .Where(i => !string.IsNullOrEmpty(i));

            var dataModels = _modelsRespository
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => i.Configurations.Contains($"{configuration.Id}") || i.Configurations?.Any() == false)
                .Select(i => $"{i.Id}");

            var models = userModels.Concat(dataModels).Distinct();

            // exit conditions
            if (!models.Any())
            {
                return;
            }

            // build
            var modelEntities = _modelsRespository
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => models.Contains($"{i.Id}"))
                .SelectMany(i => i.Models)
                .Select(i => JsonSerializer.Serialize(i, Utilities.JsonSettings));

            // update
            configuration.Models = configuration
                .Models
                .Where(i => !GetIdPattern().IsMatch(i))
                .Concat(modelEntities);
        }

        private void SetTestsRepository(RhinoConfiguration configuration)
        {
            // setup
            var tests = configuration
                .TestsRepository
                .Select(i => GetIdPattern().Match(i).Value)
                .Where(i => !string.IsNullOrEmpty(i));

            // exit conditions
            if (!tests.Any())
            {
                return;
            }

            // cache
            var origRepository = configuration.TestsRepository.Clone();

            // build
            var stateRepository = _testsRepository
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => tests.Contains($"{i.Id}"))
                .SelectMany(i => i.RhinoTestCaseModels)
                .Select(i => i.RhinoSpec)
                .Concat(configuration.TestsRepository.Where(i => !GetIdPattern().IsMatch(i)));

            // get
            configuration.TestsRepository = configuration.ConnectorConfiguration.Connector != RhinoConnectors.Text && !stateRepository.Any()
                ? origRepository
                : stateRepository;
        }

        private void SetSettings(RhinoConfiguration configuration)
        {
            // constants
            const string ReportsOut = "Rhino:ReportConfiguration:ReportsOut";
            const string LogsOut = "Rhino:ReportConfiguration:LogsOut";
            const string Archive = "Rhino:ReportConfiguration:Archive";
            const string Reporters = "Rhino:ReportConfiguration:Reporters";
            const string ScreenshotsOut = "Rhino:ScreenshotsConfiguration:ScreenshotsOut";
            const string KeepOriginal = "Rhino:ScreenshotsConfiguration:KeepOriginal";

            // configuration
            var reporters = configuration.ReportConfiguration.Reporters ?? Array.Empty<string>();

            // reporting
            configuration.ReportConfiguration.ReportOut = _configuration.GetValue(ReportsOut, defaultValue: ".");
            configuration.ReportConfiguration.LogsOut = _configuration.GetValue(LogsOut, defaultValue: ".");
            configuration.ReportConfiguration.Archive = _configuration.GetValue(Archive, defaultValue: false);
            configuration.ReportConfiguration.Reporters = _configuration
                .GetSection(Reporters)
                .GetChildren()
                .Select(i => i.Value)
                .Concat(reporters)
                .Where(i => !string.IsNullOrEmpty(i))
                .Distinct()
                .ToArray();

            // screen-shots
            configuration.ScreenshotsConfiguration.ScreenshotsOut = _configuration.GetValue(ScreenshotsOut, defaultValue: ".");
            configuration.ScreenshotsConfiguration.KeepOriginal = _configuration.GetValue(KeepOriginal, defaultValue: false);
        }
    }
}
