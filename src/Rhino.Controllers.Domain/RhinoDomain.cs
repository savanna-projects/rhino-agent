/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Services.Comet;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Domain.Automation;
using Rhino.Controllers.Domain.Data;
using Rhino.Controllers.Domain.Integration;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Domain.Orchestrator;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using System.Collections.Concurrent;

namespace Rhino.Controllers.Domain
{
    public class RhinoDomain : IDomain
    {
        /// <summary>
        /// Create a new instance of RhinoDomain.
        /// </summary>
        /// <param name="application">An IApplicationRepository implementation to use with RhinoDomain.</param>
        /// <param name="appSettings">An IConfiguration implementation to use with RhinoDomain.</param>
        /// <param name="configurations">An IRepository<RhinoConfiguration> implementation to use with RhinoDomain.</param>
        /// <param name="environments">An IEnvironmentRepository implementation to use with RhinoDomain.</param>
        /// <param name="hub">An IEnvironmentRepository implementation to use with RhinoDomain.</param>
        /// <param name="logs">An ILogsRepository implementation to use with RhinoDomain.</param>
        /// <param name="metaData">An IHubRepository implementation to use with RhinoDomain.</param>
        /// <param name="models">An IRepository<RhinoModelCollection> implementation to use with RhinoDomain.</param>
        /// <param name="plugins">An IPluginsRepository implementation to use with RhinoDomain.</param>
        /// <param name="rhino">An IRhinoRepository implementation to use with RhinoDomain.</param>
        /// <param name="rhinoAsync">An IRhinoAsyncRepository implementation to use with RhinoDomain.</param>
        /// <param name="tests">An ITestsRepository implementation to use with RhinoDomain.</param>
        public RhinoDomain(
            IApplicationRepository application,
            AppSettings appSettings,
            IRepository<RhinoConfiguration> configurations,
            IEnvironmentRepository environments,
            IHubRepository hub,
            ILogsRepository logs,
            IMetaDataRepository metaData,
            IRepository<RhinoModelCollection> models,
            IPluginsRepository plugins,
            IRhinoRepository rhino,
            IRhinoAsyncRepository rhinoAsync,
            ITestsRepository tests)
        {
            Application = application;
            AppSettings = appSettings;
            Configurations = configurations;
            Environments = environments;
            Hub = hub;
            Logs = logs;
            MetaData = metaData;
            Models = models;
            Plugins = plugins;
            Rhino = rhino;
            RhinoAsync = rhinoAsync;
            Tests = tests;
        }

        public IApplicationRepository Application { get; set; }
        public AppSettings AppSettings { get; set; }
        public IRepository<RhinoConfiguration> Configurations { get; set; }
        public IEnvironmentRepository Environments { get; set; }
        public IHubRepository Hub { get; }
        public ILogsRepository Logs { get; set; }
        public IMetaDataRepository MetaData { get; set; }
        public IRepository<RhinoModelCollection> Models { get; set; }
        public IPluginsRepository Plugins { get; set; }
        public IRhinoRepository Rhino { get; set; }
        public IRhinoAsyncRepository RhinoAsync { get; set; }
        public ITestsRepository Tests { get; set; }

        public static void CreateDependencies(WebApplicationBuilder builder)
        {
            // setup
            var comparer = StringComparer.OrdinalIgnoreCase;

            // hub
            builder.Services.AddSingleton(typeof(IDictionary<string, TestCaseQueueModel>), new ConcurrentDictionary<string, TestCaseQueueModel>(comparer));
            builder.Services.AddSingleton(new ConcurrentQueue<TestCaseQueueModel>());
            builder.Services.AddSingleton(typeof(IDictionary<string, WebAutomation>), new ConcurrentDictionary<string, WebAutomation>(comparer));
            builder.Services.AddSingleton(new ConcurrentQueue<WebAutomation>());
            builder.Services.AddSingleton(new ConcurrentQueue<RhinoTestRun>());
            builder.Services.AddSingleton(typeof(AppSettings));
            builder.Services.AddSingleton(typeof(IDictionary<string, RhinoTestRun>), new ConcurrentDictionary<string, RhinoTestRun>(comparer));
            builder.Services.AddSingleton(typeof(IDictionary<string, WorkerQueueModel>), new ConcurrentDictionary<string, WorkerQueueModel>(comparer));
            builder.Services.AddTransient<IHubRepository, HubRepository>();

            // utilities
            builder.Services.AddTransient(typeof(ILogger), (_) => ControllerUtilities.GetLogger(builder.Configuration));
            builder.Services.AddTransient(typeof(Orbit), (_) => new Orbit(Utilities.Types));
            builder.Services.AddSingleton(typeof(IEnumerable<Type>), Utilities.Types);
            builder.Services.AddSingleton(typeof(ConcurrentBag<(RhinoTestCase TestCase, IDictionary<string, object>)>), new ConcurrentBag<(RhinoTestCase TestCase, IDictionary<string, object>)>());

            // data
            builder.Services.AddLiteDatabase(builder.Configuration.GetValue<string>("Rhino:StateManager:DataEncryptionKey"));

            // domain
            builder.Services.AddTransient<IEnvironmentRepository, EnvironmentRepository>();
            builder.Services.AddTransient<ILogsRepository, LogsRepository>();
            builder.Services.AddTransient<IPluginsRepository, PluginsRepository>();
            builder.Services.AddTransient<IRepository<RhinoConfiguration>, ConfigurationsRepository>();
            builder.Services.AddTransient<IRepository<RhinoModelCollection>, ModelsRepository>();
            builder.Services.AddTransient<IApplicationRepository, ApplicationRepository>();
            builder.Services.AddTransient<IRhinoAsyncRepository, RhinoRepository>();
            builder.Services.AddTransient<IRhinoRepository, RhinoRepository>();
            builder.Services.AddTransient<IMetaDataRepository, MetaDataRepository>();
            builder.Services.AddTransient<ITestsRepository, TestsRepository>();
            builder.Services.AddTransient<IHubRepository, HubRepository>();
            builder.Services.AddTransient<IDomain, RhinoDomain>();
            builder.Services.AddTransient<IWorkerRepository, WorkerRepository>();
        }
    }
}
