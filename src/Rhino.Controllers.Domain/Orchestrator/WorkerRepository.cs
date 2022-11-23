using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

using Gravity.Abstraction.Cli;

namespace Rhino.Controllers.Domain.Orchestrator
{
    public class WorkerRepository
    {
        private readonly AppSettings _appSettings;

        public WorkerRepository(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void StartConnection()
        {
            // setup
            var url = $"{_appSettings.Worker.HubAddress}/api/v{_appSettings.Worker.HubApiVersion}/rhino/orchestrator";
            var connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();
        }

        /// <summary>
        /// Try to get the endpoint for Rhino Hub based on configuration and/or command-line arguments.
        /// </summary>
        /// <param name="configuration">The configuration implementation to use.</param>
        /// <param name="cli">The command line arguments to use.</param>
        /// <returns>Rhino Hub endpoint</returns>
        public static (string HubEndpoint, string HubAddress) GetHubEndpoint(IConfiguration configuration, string cli)
        {
            // extract values
            var hubAddress = configuration.GetValue<string>("Rhino:WrokerConfiguration:HubAddress");
            var hubVersion = configuration.GetValue<int>("Rhino:WrokerConfiguration:HubApiVersion");

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
