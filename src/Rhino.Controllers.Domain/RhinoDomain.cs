/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.Extensions.Configuration;

using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Models;

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
        /// <param name="logs">An ILogsRepository implementation to use with RhinoDomain.</param>
        /// <param name="metaData">An IMetaDataRepository implementation to use with RhinoDomain.</param>
        /// <param name="models">An IRepository<RhinoModelCollection> implementation to use with RhinoDomain.</param>
        /// <param name="plugins">An IPluginsRepository implementation to use with RhinoDomain.</param>
        /// <param name="rhino">An IRhinoRepository implementation to use with RhinoDomain.</param>
        /// <param name="rhinoAsync">An IRhinoAsyncRepository implementation to use with RhinoDomain.</param>
        /// <param name="tests">An ITestsRepository implementation to use with RhinoDomain.</param>
        public RhinoDomain(
            IApplicationRepository application,
            IConfiguration appSettings,
            IRepository<RhinoConfiguration> configurations,
            IEnvironmentRepository environments,
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
            Logs = logs;
            MetaData = metaData;
            Models = models;
            Plugins = plugins;
            Rhino = rhino;
            RhinoAsync = rhinoAsync;
            Tests = tests;
        }

        public IApplicationRepository Application { get; set; }
        public IConfiguration AppSettings { get; set; }
        public IRepository<RhinoConfiguration> Configurations { get; set; }
        public IEnvironmentRepository Environments { get; set; }
        public ILogsRepository Logs { get; set; }
        public IMetaDataRepository MetaData { get; set; }
        public IRepository<RhinoModelCollection> Models { get; set; }
        public IPluginsRepository Plugins { get; set; }
        public IRhinoRepository Rhino { get; set; }
        public IRhinoAsyncRepository RhinoAsync { get; set; }
        public ITestsRepository Tests { get; set; }
    }
}
