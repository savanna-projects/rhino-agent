/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.SignalR.Client;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Domain.Middleware;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;
using Rhino.Settings;

using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace Rhino.Controllers.Domain.Orchestrator
{
    public class WorkerRepository : IWorkerRepository
    {
        // members: static
        private static CancellationTokenSource s_tokenSource = new();
        private readonly static JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // members
        private readonly AppSettings _settings;
        private readonly ConcurrentBag<(RhinoTestCase TestCase, IDictionary<string, object> Context)> _repairs;
        private bool workerLock;

        /// <summary>
        /// Initialize a new instance of WorkerRepository class.
        /// </summary>
        /// <param name="settings">The application settings object.</param>
        public WorkerRepository(AppSettings settings)
            : this(settings, new ConcurrentBag<(RhinoTestCase TestCase, IDictionary<string, object> Context)>(), string.Empty)
        { }

        /// <summary>
        /// Initialize a new instance of WorkerRepository class.
        /// </summary>
        /// <param name="settings">The application settings object.</param>
        public WorkerRepository(AppSettings settings, ConcurrentBag<(RhinoTestCase TestCase, IDictionary<string, object> Context)> repairs)
            : this(settings, repairs, string.Empty)
        { }

        /// <summary>
        /// Initialize a new instance of WorkerRepository class.
        /// </summary>
        /// <param name="settings">The application settings object.</param>
        /// <param name="cli">A command line integration phrase to use with the repository.</param>
        public WorkerRepository(AppSettings settings, ConcurrentBag<(RhinoTestCase TestCase, IDictionary<string, object> Context)> repairs, string cli)
        {
            // setup
            _settings = settings;
            _repairs = repairs;
            var (hubEndpoint, _, _) = settings.GetHubEndpoints(cli);

            // setup connection
            Connection = SetConnection(hubEndpoint);
        }

        /// <summary>
        /// Gets the underline HubConnection object.
        /// </summary>
        public HubConnection Connection { get; private set; }

        #region *** Worker Sync ***
        /// <summary>
        /// Sync all dynamic data from the connected hub (equivalent to packages restore).
        /// </summary>
        /// <remarks>Sync will first clean all existing data and will override it.</remarks>
        public static async Task SyncDataAsync(
            string baseUrl,
            IRepository<RhinoModelCollection> models,
            IEnvironmentRepository environment,
            IResourcesRepository resources,
            TimeSpan timeout)
        {
            // setup
            var client = new HttpClient();

            // sync data
            await SyncPluginsAsync(client, baseUrl, timeout);
            await SyncModelsAsync(domain: models, client, baseUrl, timeout);
            await SyncEnvironmentAsync(domain: environment, client, baseUrl, timeout);
            await SyncResourcesAsync(domain: resources, client, baseUrl, timeout);
        }

        private static async Task SyncPluginsAsync(HttpClient client, string baseUrl, TimeSpan timeout)
        {
            // constants
            const string FileName = "Plugins.zip";
            const string DirectoryName = "Plugins";

            // setup
            var pluginsPath = Path.Combine(Environment.CurrentDirectory, DirectoryName);
            var requestUri = $"{baseUrl}/plugins/export";

            // invoke
            var response = await client.GetAsync(requestUri, timeout);
            if (!response.IsSuccessStatusCode)
            {
                Trace.TraceWarning($"Sync-Plugins -Url {baseUrl} = {response.StatusCode}");
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

        private static async Task SyncModelsAsync(IRepository<RhinoModelCollection> domain, HttpClient client, string baseUrl, TimeSpan timeout)
        {
            // cleanup
            domain.Delete();

            // setup
            var requestUri = $"{baseUrl}/models";

            // invoke
            var response = await client.GetAsync(requestUri, timeout);
            if (!response.IsSuccessStatusCode)
            {
                Trace.TraceWarning($"Sync-Models -Url {baseUrl} = {response.StatusCode}");
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
                requestUri = $"{baseUrl}/models/{id}";
                if (!response.IsSuccessStatusCode)
                {
                    Trace.TraceWarning($"Sync-Model -Url {baseUrl} -Id {id} = {response.StatusCode}");
                    continue;
                }
                response = await client.GetAsync(requestUri, timeout);
                jsonData = await response.Content.ReadAsStringAsync();

                var model = JsonSerializer.Deserialize<RhinoModelCollection>(jsonData, s_jsonOptions);
                domain.Add(model);
                Trace.TraceInformation($"Sync-Model -Url {baseUrl} -Id {id} = OK");
            }
        }

        private static async Task SyncEnvironmentAsync(IEnvironmentRepository domain, HttpClient client, string baseUrl, TimeSpan timeout)
        {
            // cleanup
            domain.Delete();

            // setup
            var requestUri = $"{baseUrl}/environment";

            // invoke
            var response = await client.GetAsync(requestUri, timeout);
            if (!response.IsSuccessStatusCode)
            {
                Trace.TraceWarning($"Sync-Environment -Url {baseUrl} = {response.StatusCode}");
                return;
            }

            // build
            var jsonData = await response.Content.ReadAsStringAsync();

            // iterate
            foreach (var item in JsonSerializer.Deserialize<IDictionary<string, object>>(jsonData, s_jsonOptions))
            {
                domain.Add(item);
            }
        }

        private static async Task SyncResourcesAsync(IResourcesRepository domain, HttpClient client, string baseUrl, TimeSpan timeout)
        {
            // cleanup
            domain.Delete();

            // setup
            var requestUri = $"{baseUrl}/resources";

            // invoke
            var response = await client.GetAsync(requestUri, timeout);
            if (!response.IsSuccessStatusCode)
            {
                Trace.TraceWarning($"Sync-Resources -Url {baseUrl} = {response.StatusCode}");
                return;
            }

            // build
            var jsonData = await response.Content.ReadAsStringAsync();

            // iterate
            foreach (var item in JsonSerializer.Deserialize<IEnumerable<ResourceFileModel>>(jsonData, s_jsonOptions))
            {
                domain.Create(item);
            }
        }
        #endregion

        /// <summary>
        /// Stops the worker listener from collection new test cases.
        /// </summary>
        /// <remarks>The running tests will complete.</remarks>
        public void StopWorker() => s_tokenSource.Cancel();

        /// <summary>
        /// Restart the worker listener collection new test cases.
        /// </summary>
        /// <remarks>Use if stop request was sent and you wish to restart the worker.</remarks>
        public void RestartWorker() => s_tokenSource = new();

        /// <summary>
        /// Starts the worker listener, collecting test cases from the hub.
        /// </summary>
        /// <remarks>This is a long running action and will block the application main thread.</remarks>
        public void StartWorker()
        {
            // iterate
            while (!s_tokenSource.IsCancellationRequested)
            {
                if (Connection.State != HubConnectionState.Connected)
                {
                    StartConnection(_settings);
                }
                try
                {
                    if (!workerLock)
                    {
                        workerLock = true;
                        Connection.InvokeAsync("get").GetAwaiter().GetResult();
                    }
                }
                catch (Exception e) when (e != null)
                {
                    Trace.TraceError("Start-Worker" +
                        $"-Connection {Connection.ConnectionId} = (Error | {e.GetBaseException().Message})");
                }
                Thread.Sleep(3000);
            }
        }

        /// <summary>
        /// Gets the worker status (Disabled or Enabled).
        /// </summary>
        /// <returns>The worker status.</returns>
        public string GetWorkerStatus()
        {
            return workerLock ? "Running" : "Idle";
        }

        // Connection Methods
        private static void OnPing(string message) => Trace.TraceInformation($"Pong: {message}");

        private static void OnGet(
            HubConnection connection,
            RhinoConfiguration configuration,
            RhinoTestCase testCase,
            IDictionary<string, object> context,
            ConcurrentBag<(RhinoTestCase TestCase, IDictionary<string, object> Context)> repairs)
        {
            // invoke
            try
            {
                new InvokeTestCaseMiddleware(connection, configuration, repairs)
                    .Invoke(testCase, context, "update")
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e) when (e != null)
            {
                Trace.TraceError("Invoke-TestCase " +
                    $"-Connection {connection?.ConnectionId} " +
                    $"-TestCase {testCase.Identifier} " +
                    $"-Configuration {configuration.Name} = (Error | {e.GetBaseException().Message})");
            }
        }

        // Connection Life-Cycle
        private HubConnection SetConnection(string hubEndpoint)
        {
            // build
            var connection = new HubConnectionBuilder()
                .WithUrl(hubEndpoint)
                .Build();

            // setup: delegates
            connection.KeepAliveInterval = TimeSpan.FromSeconds(5);
            connection.Reconnected += (args) => Reconnected(connection, args);
            connection.Reconnecting += (e) => Reconnecting(connection, e);
            connection.Closed += (e) =>
            {
                workerLock = false;
                return Closed(e);
            };

            // setup: api
            connection.On<string>("ping", OnPing);
            connection.On<RhinoConfiguration, RhinoTestCase, IDictionary<string, object>>("get", (configuration, testCase, context) =>
            {
                OnGet(connection, configuration, testCase, context, _repairs);
                workerLock = false;
            });
            connection.On("404", () => workerLock = false);

            // get
            return connection;
        }

        private void StartConnection(AppSettings settings)
        {
            // setup
            var isConnected = false;
            var timeout = DateTime.Now.AddSeconds(settings.Worker.ConnectionTimeout);

            // attempt connection
            while (!isConnected && DateTime.Now < timeout)
            {
                try
                {
                    Connection.StartAsync().Wait();
                    isConnected = true;
                    foreach (var (testCase, context) in _repairs)
                    {
                        Connection
                            .InvokeAsync("repair", testCase, context)
                            .GetAwaiter()
                            .GetResult();
                        Trace.TraceInformation($"Repair-TestCase -Key {testCase.Key} = OK");
                    }
                    Trace.TraceInformation($"Connect-Hub = {Connection?.State}");
                }
                catch (Exception e) when (e.GetBaseException() is not InvalidOperationException)
                {
                    Connection.StopAsync().Wait();
                    Trace.TraceError($"Connect-Hub = (Error | {e.GetBaseException().Message})");
                    Thread.Sleep(5000);
                }
                catch (Exception e) when (e.GetBaseException() is InvalidOperationException)
                {
                    var dispose = Connection.DisposeAsync();
                    var isDispose = dispose.IsCompleted;
                    while (!isDispose && DateTime.Now < timeout)
                    {
                        Thread.Sleep(1000);
                        isDispose = dispose.IsCompleted;
                    }
                    if (dispose.IsCompleted)
                    {
                        Connection = null;
                        Connection = SetConnection(hubEndpoint: settings.GetHubEndpoints().HubEndpoint);
                    }
                }
            }
        }

        // Connection Events
        private static Task Reconnecting(HubConnection connection, Exception e) => Task.Factory.StartNew(() =>
        {
            var message = "Connect-Hub " +
                $"-Id {connection?.ConnectionId} " +
                $"-Event 'Reconnecting' = {(e == null ? "OK" : $"(OK | {e?.GetBaseException().Message})")}";
            Trace.TraceInformation(message);
        });

        private static Task Reconnected(HubConnection connection, string args) => Task.Factory.StartNew(() =>
        {
            var message = "Connect-Hub " +
                $"-Id {connection?.ConnectionId} " +
                (string.IsNullOrEmpty(args) ? string.Empty : $"-Arguments {args} ") +
                "-Event 'Reconnected' = InProgress";
            Trace.TraceInformation(message);
        });

        private Task Closed(Exception e) => Task.Factory.StartNew(() =>
        {
            // log
            var message = "Connect-Hub " +
                $"-Id {Connection?.ConnectionId} " +
                $"-Event 'Closed' = {(e == null ? "Error" : $"(Error | {e?.GetBaseException().Message})")}";
            Trace.TraceInformation(message);
        });
    }
}
