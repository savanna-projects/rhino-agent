/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Extensions;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rhino.Controllers.Domain.Automation
{
    /// <summary>
    /// Data Access Layer for Rhino API and integration operations.
    /// </summary>
    public class RhinoRepository : IRhinoRepository, IRhinoAsyncRepository
    {
        // constants
        private const string IdPattern = @"^\w{8}-(\w{4}-){3}\w{12}$";
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

        // members: cache
        private readonly static IDictionary<string, AsyncStatusModel<RhinoConfiguration>> status
            = new ConcurrentDictionary<string, AsyncStatusModel<RhinoConfiguration>>();

        // members: state
        private readonly IEnumerable<Type> types;
        private readonly IRepository<RhinoConfiguration> configurationsRepository;
        private readonly IRepository<RhinoModelCollection> modelsRespository;
        private readonly ITestsRepository testsRepository;
        private readonly ILogger logger;

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
            ILogger logger)
        {
            this.types = types;
            this.configurationsRepository = configurationsRepository;
            this.modelsRespository = modelsRespository;
            this.testsRepository = testsRepository;
            this.logger = logger.CreateChildLogger(nameof(RhinoRepository));
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
        public (int StatusCode, RhinoTestRun TestRun) InvokeConfiguration(RhinoConfiguration configuration)
        {
            // setup
            var (statusCode, entity) = BuildConfiguration(configuration);

            // failed
            if (statusCode != StatusCodes.Status200OK)
            {
                return (statusCode, default);
            }

            // get
            return DoInvoke(configurations: new[] { entity }).FirstOrDefault();
        }

        /// <summary>
        /// Invokes (run) a RhinoConfiguration.
        /// </summary>
        /// <param name="configuration">The RhinoConfiguration.Id to invoke.</param>
        /// <returns>Status code and RhinoTestRun object (if any).</returns>
        public (int StatusCode, RhinoTestRun TestRun) InvokeConfiguration(string configuration)
        {
            // setup
            var (statusCode, entity) = configurationsRepository
                .SetAuthentication(Authentication)
                .Get(configuration);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return (StatusCodes.Status404NotFound, null);
            }

            // build
            entity.Id = $"{entity.Id}".Equals(configuration, Compare) ? entity.Id : Guid.Parse(configuration);
            (statusCode, entity) = BuildConfiguration(entity);

            // error
            if (statusCode != StatusCodes.Status200OK)
            {
                return (statusCode, default);
            }

            // get
            return DoInvoke(configurations: new[] { entity }).FirstOrDefault();
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
        public IEnumerable<(int StatusCode, RhinoTestRun TestRun)> InvokeCollection(string collection, bool isParallel, int maxParallel)
        {
            // setup
            var (statusCode, entity) = testsRepository.SetAuthentication(Authentication).Get(collection);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                logger?.Warn($"Invoke-Collection -Id {collection} = (NotFound, Collection)");
                return new (int StatusCode, RhinoTestRun TestRun)[] { (statusCode, default) };
            }
            if (entity.Configurations.Count == 0)
            {
                logger?.Warn($"Invoke-Collection -Id {collection} = (NotFound, Configurations)");
                return new (int StatusCode, RhinoTestRun TestRun)[] { (statusCode, default) };
            }

            // build
            var sourceConfigurations = entity.Configurations.Select(i => i.ToUpper()).ToArray();
            var configurations = configurationsRepository
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => sourceConfigurations.Contains($"{i.Id}".ToUpper()))
                .Select(BuildConfiguration)
                .Where(i => i.StatusCode == StatusCodes.Status200OK)
                .Select(i => i.Configuration);

            // get
            return DoInvoke(configurations, isParallel, maxParallel);
        }

        private IEnumerable<(int StatusCode, RhinoTestRun Results)> DoInvoke(
            IEnumerable<RhinoConfiguration> configurations,
            bool isParallel = false,
            int maxParallel = 0)
        {
            // setup
            maxParallel = isParallel ? maxParallel : 1;
            maxParallel = maxParallel < 1 ? Environment.ProcessorCount : maxParallel;
            var options = new ParallelOptions { MaxDegreeOfParallelism = maxParallel };
            var results = new ConcurrentBag<(int StatusCode, RhinoTestRun Results)>();

            // invoke
            Parallel.ForEach(configurations, options, configuration =>
            {
                try
                {
                    var responseBody = configuration.Execute(types);
                    logger?.Debug($"invoke-Configuration = {responseBody.Key}");
                    results.Add((StatusCodes.Status200OK, responseBody));
                }
                catch (Exception e) when (e != null)
                {
                    logger?.Debug($"invoke-Configuration = (InternalServerError, ({e.GetBaseException().Message})");
                    results.Add((StatusCodes.Status500InternalServerError, default));
                }
            });

            // get
            return results;
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
            var (statusCode, entity) = BuildConfiguration(configuration);

            // error
            if (statusCode != StatusCodes.Status200OK)
            {
                return new AsyncInvokeModel { Id = default, StatusCode = statusCode, StatusEndpoint = default };
            }

            // build
            var (action, model) = DoStart(entity);

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
            var (statusCode, entity) = configurationsRepository.SetAuthentication(Authentication).Get(configuration);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return new AsyncInvokeModel { Id = default, StatusCode = StatusCodes.Status404NotFound, StatusEndpoint = default };
            }

            // build
            entity.Id = $"{entity.Id}".Equals(configuration, Compare) ? entity.Id : Guid.Parse(configuration);
            (statusCode, entity) = BuildConfiguration(entity);

            // error
            if (statusCode != StatusCodes.Status200OK)
            {
                return new AsyncInvokeModel { Id = default, StatusCode = statusCode, StatusEndpoint = default };
            }

            // build
            var (action, model) = DoStart(entity);

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
            var (statusCode, entity) = testsRepository.SetAuthentication(Authentication).Get(collection);

            // not found
            var notFound = new AsyncInvokeModel
            {
                Id = default,
                StatusCode = StatusCodes.Status404NotFound,
                StatusEndpoint = default
            };
            if (statusCode == StatusCodes.Status404NotFound)
            {
                logger?.Warn($"Start-Collection -Id {collection} = (NotFound, Collection)");
                return new[] { notFound };
            }
            if (entity.Configurations.Count == 0)
            {
                logger?.Warn($"Start-Collection -Id {collection} = (NotFound, Configurations)");
                return new[] { notFound };
            }

            // build
            var sourceConfigurations = entity.Configurations.Select(i => i.ToUpper()).ToArray();
            var configurations = configurationsRepository
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => sourceConfigurations.Contains($"{i.Id}".ToUpper()))
                .Select(BuildConfiguration)
                .Where(i => i.StatusCode == StatusCodes.Status200OK)
                .Select(i => DoStart(i.Configuration))
                .ToArray();

            // invoke
            Task.Run(() => Parallel.ForEach(configurations.Select(i => i.Action), options, action => action.RunSynchronously()));

            // get
            return configurations.Select(i => i.Model);
        }

        public (Task Action, AsyncInvokeModel Model) DoStart(RhinoConfiguration configuration)
        {
            // setup
            var id = Guid.NewGuid();
            var endpoint = $"/api/v3/rhino/async/status/{id}";

            // register
            status[$"{id}"] = new AsyncStatusModel<RhinoConfiguration>
            {
                EntityOut = default,
                Progress = default,
                Status = AsyncStatus.Pending,
                RuntimeId = $"{id}"
            };

            // delegate
            var action = new Task(() =>
            {
                status[$"{id}"].Status = AsyncStatus.Running;
                var (StatusCode, TestRun) = DoInvoke(new[] { configuration }).First();

                status[$"{id}"] = new AsyncStatusModel<RhinoConfiguration>
                {
                    EntityOut = TestRun,
                    Progress = 100,
                    Status = StatusCode != StatusCodes.Status200OK ? AsyncStatus.Failed : AsyncStatus.Complete,
                    StatusCode = StatusCode,
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
            return status.Select(i => i.Value);
        }

        /// <summary>
        /// Gets an async invoke status from the application cache.
        /// </summary>
        /// <param name="id">The invoke ID by which to get status.</param>
        /// <returns>Status code and AsyncStatusModel object (if any).</returns>
        public (int StatusCode, AsyncStatusModel<RhinoConfiguration> Status) GetStatus(Guid id)
        {
            return status.ContainsKey($"{id}")
                ? (StatusCodes.Status200OK, status[$"{id}"])
                : (StatusCodes.Status404NotFound, default);
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
            status.Remove(id);

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
            status.Clear();

            // get
            return StatusCodes.Status204NoContent;
        }
        #endregion

        // Build Pipeline
        private (int StatusCode, RhinoConfiguration Configuration) BuildConfiguration(RhinoConfiguration configuration)
        {
            // validation
            var validationCode = BuidValidation(configuration);
            if (validationCode != -1)
            {
                return (validationCode, configuration);
            }

            //  setup conditions
            var isUser = Authentication.UserName != string.Empty;

            // build
            configuration.Authentication = isUser ? Authentication : configuration.Authentication;

            // build
            SetModels(configuration);
            SetTestsRepository(configuration);

            // invoke
            try
            {
                logger?.Debug("Create-Configuration = Ok");
                return (StatusCodes.Status200OK, configuration);
            }
            catch (Exception e) when (e != null)
            {
                logger?.Debug($"Create-Configuration = (InternalServerError, ({e.GetBaseException().Message})");
                return (StatusCodes.Status500InternalServerError, configuration);
            }
        }

        private int BuidValidation(RhinoConfiguration configuration)
        {
            // bad request            
            if (!configuration.TestsRepository.Any())
            {
                logger?.Debug("Invoke-Configuration = (BadRequest, NoTests)");
                return StatusCodes.Status400BadRequest;
            }
            if (!configuration.DriverParameters.Any())
            {
                logger?.Debug("Invoke-Configuration = (BadRequest, NoDrivers)");
                return StatusCodes.Status400BadRequest;
            }

            // get
            return -1;
        }

        private void SetModels(RhinoConfiguration configuration)
        {
            // setup
            var userModels = configuration
                .Models
                .Select(i => Regex.Match(i, IdPattern).Value)
                .Where(i => !string.IsNullOrEmpty(i));

            var dataModels = modelsRespository
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
            var modelEntities = modelsRespository
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => models.Contains($"{i.Id}"))
                .SelectMany(i => i.Models)
                .Select(i => JsonSerializer.Serialize(i, Api.Extensions.Utilities.JsonSettings));

            // update
            configuration.Models = configuration.Models.Where(i => !Regex.IsMatch(i, IdPattern)).Concat(modelEntities);
        }

        private void SetTestsRepository(RhinoConfiguration configuration)
        {
            // setup
            var tests = configuration
                .TestsRepository
                .Select(i => Regex.Match(i, IdPattern).Value)
                .Where(i => !string.IsNullOrEmpty(i));

            // exit conditions
            if (!tests.Any())
            {
                return;
            }

            // build
            configuration.TestsRepository = testsRepository
                .SetAuthentication(Authentication)
                .Get()
                .Where(i => tests.Contains($"{i.Id}"))
                .SelectMany(i => i.RhinoTestCaseModels)
                .Select(i => i.RhinoSpec)
                .Concat(configuration.TestsRepository.Where(i => !Regex.IsMatch(i, IdPattern)));
        }
    }
}