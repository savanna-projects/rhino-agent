/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Controllers.Models;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rhino.Controllers.Hubs
{
    public class RhinoHub : Hub
    {
        // members: injection
        private readonly ConcurrentQueue<TestCaseQueueModel> _rhinoPending;
        private readonly IDictionary<string, TestCaseQueueModel> _rhinoRunning;
        private readonly ConcurrentQueue<WebAutomation> _gravityPending;
        private readonly IDictionary<string, WebAutomation> _gravityRunning;
        private readonly ILogger _logger;

        public RhinoHub(
            ConcurrentQueue<TestCaseQueueModel> rhinoPending,
            IDictionary<string, TestCaseQueueModel> rhinoRunning,
            ConcurrentQueue<WebAutomation> gravityPending,
            IDictionary<string, WebAutomation> gravityRunning,
            ILogger logger)
        {
            _rhinoPending = rhinoPending;
            _rhinoRunning = rhinoRunning;
            _gravityPending = gravityPending;
            _gravityRunning = gravityRunning;
            _logger = logger;
        }

        /// <summary>
        /// Basic method which returns "pong" response to caller client.
        /// </summary>
        [HubMethodName("ping")]
        public Task Ping()
        {
            // log to console
            _logger.Info($"Invoke-Heartbeat -Connection {Context.ConnectionId} = OK");

            // communicate back
            return Clients.Caller.SendAsync("ping", "pong");
        }

        /// <summary>
        /// Gets a test from the pending queue.
        /// </summary>
        [HubMethodName("get")]
        public async Task Get()
        {
            // setup
            var isItem = _rhinoPending.TryDequeue(out TestCaseQueueModel item);

            // exit conditions
            if (!isItem)
            {
                return;
            }

            // setup
            var feature = Context.Features.Get<IHttpConnectionFeature>();
            var remoteAddress = $"{feature?.RemoteIpAddress}";
            var ip = $"{(remoteAddress.Equals("::1") ? "localhost" : remoteAddress)}";
            var port = feature == default ? 0 : feature.RemotePort;

            // build
            item.Worker ??= new WorkerQueueModel();
            item.Worker.Address = ip;
            item.Worker.ConnectionId = Context.ConnectionId;
            item.Worker.Port = port;

            // move to running queue
            item.TestCase.Context[nameof(Context.ConnectionId)] = Context.ConnectionId;
            _rhinoRunning[item.TestCase.Identifier] = item;

            // communicate back
            await Clients
                .Caller
                .SendAsync("get", item.Connector.ProviderManager.Configuration, item.TestCase, item.TestCase.Context)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Update a RhinoTestCase status and push it back to the connector.
        /// </summary>
        /// <param name="testCase">The RhinoTestCase to update.</param>
        /// <param name="context">The RhinoTestCase context back from the worker.</param>
        [HubMethodName("update")]
        public void Update(RhinoTestCase testCase, IDictionary<string, object> context)
        {
            // not found
            if (!_rhinoRunning.ContainsKey(testCase.Identifier))
            {
                return;
            }

            // setup
            var entity = _rhinoRunning[testCase.Identifier];

            // update context
            testCase.Context = context;

            // push back to connector
            entity.Connector.ReceiveTest(testCase);

            // update running queue
            _rhinoRunning.Remove(testCase.Identifier);
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
        }
    }
}
