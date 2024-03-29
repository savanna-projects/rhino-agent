﻿/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Domain.Orchestrator;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Settings;

using System.Collections.Concurrent;
using System.Diagnostics;

namespace Rhino.Controllers.Domain.Middleware
{
    /// <summary>
    /// Middleware for initializing connections to RhinoHub.
    /// </summary>
    public class StartWorkerMiddleware
    {
        // members
        private readonly AppSettings _appSettings;
        private readonly IEnvironmentRepository _environment;
        private readonly IRepository<RhinoModelCollection> _models;
        private readonly IResourcesRepository _resources;
        private readonly ConcurrentBag<(RhinoTestCase TestCase, IDictionary<string, object> Context)> _repairs;

        /// <summary>
        /// Initialize a new instance of StartWorkerMiddleware object.
        /// </summary>
        /// <param name="appSettings">The application settings.</param>
        /// <param name="environment">The environment repository implementation.</param>
        /// <param name="models">The models repository implementation.</param>
        public StartWorkerMiddleware(
            AppSettings appSettings,
            IEnvironmentRepository environment,
            IRepository<RhinoModelCollection> models,
            IResourcesRepository resources,
            ConcurrentBag<(RhinoTestCase TestCase, IDictionary<string, object> Context)> repairs)
        {
            // setup
            _appSettings = appSettings;
            _environment = environment;
            _models = models;
            _resources = resources;
            _repairs = repairs;
        }

        public void Start(params string[] args)
        {
            // setup
            var cli = "{{$ " + string.Join(" ", args) + "}}";
            var maxParallel = _appSettings.GetMaxParallel(cli);
            var (_, hubAddress, hubApiVersion) = _appSettings.GetHubEndpoints(cli);
            var baseUrl = $"{hubAddress}/api/v{hubApiVersion}";
            var timeout = _appSettings.GetConnectionTimeout(cli);

            // sync
            WorkerRepository.SyncDataAsync(baseUrl, _models, _environment, _resources, timeout).GetAwaiter().GetResult();
            Trace.TraceInformation("Sync-Worker = OK");

            // start connections
            for (int i = 0; i < maxParallel; i++)
            {
                var repository = new WorkerRepository(_appSettings, _repairs, cli);
                Task.Run(repository.StartWorker);
            }
        }
    }
}
