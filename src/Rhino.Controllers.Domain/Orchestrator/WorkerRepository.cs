/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Cli;

using Microsoft.AspNetCore.SignalR.Client;

using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Models;

using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace Rhino.Controllers.Domain.Orchestrator
{
    public class WorkerRepository
    {
        // members: static
        private readonly static JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        private readonly static SemaphoreSlim _semaphore = new(1, 1);

        // members
        private readonly AppSettings _appSettings;
        private readonly IDomain _domain;
        private readonly (string HubEndpoint, string HubAddress) _endpoints;

        /// <summary>
        /// Initialize all static members.
        /// </summary>
        static WorkerRepository()
        {
            _semaphore.Release();
        }

        /// <summary>
        /// Initialize a new instance of WorkerRepository class.
        /// </summary>
        /// <param name="domain">The application domain to use with the repository.</param>
        public WorkerRepository(IDomain domain)
            :this(domain, string.Empty)
        { }

        /// <summary>
        /// Initialize a new instance of WorkerRepository class.
        /// </summary>
        /// <param name="domain">The application domain to use with the repository.</param>
        /// <param name="cli">A command line integration phrase to use with the repository.</param>
        public WorkerRepository(IDomain domain, string cli)
        {
            // setup
            _appSettings = domain.AppSettings;
            _endpoints = GetHubEndpoints(cli, domain.AppSettings);
            _domain = domain;

            // setup connection
            Connection = new HubConnectionBuilder()
                .WithUrl(_endpoints.HubEndpoint)
                .Build();
            Connection.KeepAliveInterval = TimeSpan.FromSeconds(5);
            Connection.Reconnected += (args) => Task.Run(() => Trace.TraceInformation($"Connect-Hub -Id {Connection?.ConnectionId} -Reconnect = InProgress"));
            Connection.Reconnecting += (args) => Task.Run(() => Trace.TraceInformation($"Connect-Hub -Id {Connection?.ConnectionId} = OK"));
            Connection.Closed += (arg) => Task.Run(() => Trace.TraceInformation($"Close-HubConnection -Id {Connection?.ConnectionId} = (Error | {arg?.Message})"));
            Connection.StartAsync();
        }

        /// <summary>
        /// Gets the underline HubConnection object.
        /// </summary>
        public HubConnection Connection { get; }

        /// <summary>
        /// Sync all dynamic data from the connected hub (equivalent to packages restore).
        /// </summary>
        /// <remarks>Sync will first clean all existing data and will override it.</remarks>
        public async Task SyncDataAsync()
        {
            // setup
            var requestUri = $"{_endpoints.HubAddress}/api/v{_appSettings.Worker.HubApiVersion}";
            var client = new HttpClient();

            // sync data
            await SyncPluginsAsync(client, requestUri);
            await SyncModelsAsync(_domain, client, requestUri);
            await SyncEnvironmentAsync(_domain, client, requestUri);
        }

        private static async Task SyncPluginsAsync(HttpClient client, string requestUri)
        {
            // constants
            const string FileName = "Plugins.zip";
            const string DirectoryName = "Plugins";

            // setup
            var pluginsPath = Path.Combine(Environment.CurrentDirectory, DirectoryName);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{requestUri}/plugins/export");

            // invoke
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Trace.TraceWarning($"Sync-Plugins -Url {requestUri} = {response.StatusCode}");
                return;
            }

            // extract
            var bytes = await response.Content.ReadAsByteArrayAsync();

            // create plugins
            await File.WriteAllBytesAsync(FileName, bytes);
            if (Directory.Exists(pluginsPath))
            {
                Directory.Delete(pluginsPath, true);
            }
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, DirectoryName));
            ZipFile.ExtractToDirectory(FileName, Path.Combine(Environment.CurrentDirectory, DirectoryName), true);

            // cleanup
            File.Delete("Plugins.zip");
        }

        private static async Task SyncModelsAsync(IDomain domain, HttpClient client, string requestUri)
        {
            // cleanup
            domain.Models.Delete();

            // setup
            var request = new HttpRequestMessage(HttpMethod.Get, $"{requestUri}/models");

            // invoke
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Trace.TraceWarning($"Sync-Models -Url {requestUri} = {response.StatusCode}");
                return;
            }

            // build
            var jsonData = await response.Content.ReadAsStringAsync();
            var models = JsonSerializer
                .Deserialize<IEnumerable<ModelCollectionResponseModel>>(jsonData, s_jsonOptions)
                .Select(i => i.Id);

            // iterate
            foreach (var id in models)
            {
                request = new HttpRequestMessage(HttpMethod.Get, $"{requestUri}/models/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    Trace.TraceWarning($"Sync-Model -Url {requestUri} -Id {id} = {response.StatusCode}");
                    continue;
                }
                response = await client.SendAsync(request);
                jsonData = await response.Content.ReadAsStringAsync();
                
                var model = JsonSerializer.Deserialize<RhinoModelCollection>(jsonData, s_jsonOptions);
                domain.Models.Add(model);
                Trace.TraceInformation($"Sync-Model -Url {requestUri} -Id {id} = OK");
            }
        }

        private static async Task SyncEnvironmentAsync(IDomain domain, HttpClient client, string requestUri)
        {
            // cleanup
            domain.Environments.Delete();

            // setup
            var request = new HttpRequestMessage(HttpMethod.Get, $"{requestUri}/environment");

            // invoke
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Trace.TraceWarning($"Sync-Environment -Url {requestUri} = {response.StatusCode}");
                return;
            }

            // build
            var jsonData = await response.Content.ReadAsStringAsync();
            var environment = JsonSerializer.Deserialize<IDictionary<string, object>>(jsonData, s_jsonOptions);

            // iterate
            foreach (var item in environment)
            {
                domain.Environments.Add(item);
            }
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

        public void StartConnection()
        {
            var address = _appSettings.Worker.HubAddress;
            var version = _appSettings.Worker.HubApiVersion;
            var connection = new HubConnectionBuilder()
                .WithUrl($"address/api/v{version}/rhino/orchestrator")
                .Build();
        }
    }
}
