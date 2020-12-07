/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Rhino.Agent.Domain;
using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Contracts.Interfaces;
using Rhino.Api.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Rhino.Agent.Extensions
{
    /// <summary>
    /// Extension package for Rhino.Api.Contracts.Configuration.RhinoConfiguration object.
    /// </summary>
    internal static class ConfigurationExtensions
    {
        /// <summary>
        /// Apply appsettings.json onto Rhino.Api.Contracts.Configuration.RhinoConfiguration instance.
        /// </summary>
        /// <param name="configuration">Rhino.Api.Contracts.Configuration.RhinoConfiguration to apply setting to.</param>
        /// <param name="appSettings">Settings to apply from.</param>
        /// <returns>Rhino.Api.Contracts.Configuration.RhinoConfiguration after settings applied.</returns>
        public static RhinoConfiguration ApplySettings(this RhinoConfiguration configuration, IConfiguration appSettings)
        {
            // reporting
            configuration.ReportConfiguration.ReportOut =
                appSettings.GetValue<string>("rhino:reportConfiguration:reportOut");

            configuration.ReportConfiguration.LogsOut =
                appSettings.GetValue<string>("rhino:reportConfiguration:logsOut");

            configuration.ReportConfiguration.Archive =
                appSettings.GetValue<bool>("rhino:reportConfiguration:archive");

            configuration.ReportConfiguration.Reporters = appSettings
                .GetSection("rhino:reportConfiguration:reporters")
                .GetChildren()
                .Select(i => i.Value)
                .ToArray();

            // screenshots
            configuration.ScreenshotsConfiguration.ScreenshotsOut =
                appSettings.GetValue<string>("rhino:screenshotsConfiguration:screenshotsOut");

            // updated state
            return configuration;
        }

        #region *** Connector     ***
        /// <summary>
        /// Gets a connector.
        /// </summary>
        /// <param name="configuration">RhinoConfiguration by which to factor RhinoConnector</param>
        /// <returns>RhinoConnector implementation.</returns>
        public static Type GetConnector(this RhinoConfiguration configuration)
        {
            return DoGetConnector(configuration, Api.Extensions.Utilities.Types);
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

        #region *** Configuration ***
        /// <summary>
        /// Apply RhinoTestCase collection from a user repository in RhinoAgent state..
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> to build configuration from.</param>
        /// <param name="provider">Service container from which to get different repository services.</param>
        /// <returns>A collection of Rhino.Api.Contracts.Configuration.RhinoConfiguration objects.</returns>
        public static IEnumerable<RhinoConfiguration> GetConfigurations(this HttpRequest request, IServiceProvider provider)
        {
            // setup
            var configurations = request.ReadAsAsync<RhinoConfiguration[]>().GetAwaiter().GetResult();
            var authentication = request.GetAuthentication();

            // results
            return configurations.Select(i => GetConfiguration(authentication, configuration: i, provider));
        }

        private static RhinoConfiguration GetConfiguration(Authentication authentication, RhinoConfiguration configuration, IServiceProvider provider)
        {
            // models
            SetModels(authentication, configuration, provider);

            // test cases
            SetTestCases(authentication, configuration, provider);

            // result
            return configuration;
        }

        private static void SetModels(Authentication authentication, RhinoConfiguration configuration, IServiceProvider provider)
        {
            // setup
            var modelsRepository = provider.GetRequiredService<RhinoModelRepository>();

            // models
            var models = configuration
                .Models
                .Select(i => modelsRepository.Get(authentication, id: i))
                .Where(i => i.statusCode != HttpStatusCode.NotFound)
                .Select(i => JsonConvert.SerializeObject(i.data.Models));

            if (models.Any())
            {
                configuration.Models = models;
            }
        }

        private static void SetTestCases(Authentication authentication, RhinoConfiguration configuration, IServiceProvider provider)
        {
            // setup
            var testCaseRepository = provider.GetRequiredService<RhinoTestCaseRepository>();

            // test cases
            var testCases = configuration
                .TestsRepository
                .Select(i => testCaseRepository.Get(authentication, id: i))
                .Where(i => i.statusCode != HttpStatusCode.NotFound)
                .SelectMany(i => i.data.RhinoTestCaseDocuments.Select(j => j.RhinoSpec));

            if (testCases.Any())
            {
                configuration.TestsRepository = testCases;
            }
        }
        #endregion
    }
}