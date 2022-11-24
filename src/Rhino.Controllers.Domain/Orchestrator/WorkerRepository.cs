/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Cli;

using Microsoft.AspNetCore.SignalR.Client;

using System.Diagnostics;

namespace Rhino.Controllers.Domain.Orchestrator
{
    public class WorkerRepository
    {
        // members
        private readonly AppSettings _appSettings;
        private readonly (string HubEndpoint, string HubAddress) _endpoints;

        /// <summary>
        /// Initialize a new instance of WorkerRepository class.
        /// </summary>
        /// <param name="appSettings">The application settings to use with the repository.</param>
        public WorkerRepository(AppSettings appSettings)
            :this(appSettings, string.Empty)
        { }

        /// <summary>
        /// Initialize a new instance of WorkerRepository class.
        /// </summary>
        /// <param name="appSettings">The application settings to use with the repository.</param>
        /// <param name="cli">A command line integration phrase to use with the repository.</param>
        public WorkerRepository(AppSettings appSettings, string cli)
        {
            // setup
            _appSettings = appSettings;
            _endpoints = GetHubEndpoints(cli, appSettings);

            // setup connection
            Connection = new HubConnectionBuilder()
                .WithUrl(_endpoints.HubEndpoint)
                .Build();
            Connection.KeepAliveInterval = TimeSpan.FromSeconds(5);
            Connection.Reconnected += (args) => Task.Run(() => Trace.TraceInformation($"Connect-Hub -Id {Connection?.ConnectionId} -Reconnect = InProgress"));
            Connection.Reconnecting += (args) => Task.Run(() => Trace.TraceInformation($"Connect-Hub -Id {Connection?.ConnectionId} = OK"));
            Connection.Closed += (arg) => Task.Run(() => Trace.TraceInformation($"Close-HubConnection -Id {Connection?.ConnectionId} = (Error | {arg.Message})"));
            Connection.StartAsync();
        }

        /// <summary>
        /// Gets the underline HubConnection object.
        /// </summary>
        public HubConnection Connection { get; }

        public Task StartConnection()
        {

        }

        /// <summary>
        /// Try to get the endpoint for Rhino Hub based on configuration and/or command-line arguments.
        /// </summary>
        /// <param name="appSettings">The configuration implementation to use.</param>
        /// <returns>Rhino Hub endpoint information.</returns>
        public static (string HubEndpoint, string HubAddress) GetHubEndpoints(AppSettings appSettings)
        {
            return GetHubEndpoints(cli: string.Empty, appSettings);
        }

        /// <summary>
        /// Try to get the endpoint for Rhino Hub based on configuration and/or command-line arguments.
        /// </summary>
        /// <param name="appSettings">The configuration implementation to use.</param>
        /// <param name="cli">The command line arguments to use.</param>
        /// <returns>Rhino Hub endpoint information.</returns>
        public static (string HubEndpoint, string HubAddress) GetHubEndpoints(AppSettings appSettings, string cli)
        {
            return GetHubEndpoints(cli, appSettings);
        }

        private static (string HubEndpoint, string HubAddress) GetHubEndpoints(string cli, AppSettings appSettings)
        {
            // extract values
            var hubAddress = appSettings.Worker.HubAddress;
            var hubVersion = appSettings.Worker.HubApiVersion;

            // normalize
            hubAddress = string.IsNullOrEmpty(hubAddress) ? "http://localhost:9000" : hubAddress;
            hubVersion = hubVersion == 0 ? 1 : hubVersion;

            // get from command line
            if (string.IsNullOrEmpty(cli))
            {
                return ($"{hubAddress}/api/v{hubVersion}/rhino/orchestrator", hubAddress);
            }

            // parse
            var arguments = new CliFactory(cli).Parse();
            _ = arguments.TryGetValue("hubVersion", out string versionOut);
            var isHubVersion = int.TryParse(versionOut, out int hubVersionOut);

            hubAddress = arguments.TryGetValue("hubAddress", out string addressOut) ? addressOut : hubAddress;
            hubVersion = isHubVersion ? hubVersionOut : hubVersion;

            // get
            return ($"{hubAddress}/api/v{hubVersion}/rhino/orchestrator", hubAddress);
        }
    }
}
