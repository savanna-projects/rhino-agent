/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using Microsoft.AspNetCore.SignalR;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rhino.Controllers.Hubs
{
    public class RhinoHub : Hub
    {
        // members: injection
        private readonly ConcurrentQueue<TestCaseQueueModel> _rhinoPending;
        private readonly IDictionary<string, WorkerQueueModel> _workers;
        private readonly IDictionary<string, TestCaseQueueModel> _rhinoRunning;
        private readonly ILogger _logger;

        public RhinoHub(
            ConcurrentQueue<TestCaseQueueModel> rhinoPending,
            IDictionary<string, WorkerQueueModel> workers,
            IDictionary<string, TestCaseQueueModel> rhinoRunning,
            ILogger logger)
        {
            _rhinoPending = rhinoPending;
            _workers = workers;
            _rhinoRunning = rhinoRunning;
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
                await Clients.Caller.SendAsync("404").ConfigureAwait(false);
                return;
            }

            // setup
            var (ip, port) = Context.GetAddress();

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

        /// <summary>
        /// Handles an unexpected error on the worker side.
        /// </summary>
        /// <param name="testCase">The RhinoTestCase to update.</param>
        /// <param name="context">The RhinoTestCase context back from the worker.</param>
        [HubMethodName("repair")]
        public void Repair(RhinoTestCase testCase, IDictionary<string, object> context)
        {
            // not found
            if (!_rhinoRunning.ContainsKey(testCase.Identifier))
            {
                return;
            }

            // setup
            var entity = _rhinoRunning[testCase.Identifier];
            testCase.Context = context;
            entity.TestCase = testCase;

            // push back to pending to pickup by another worker
            _rhinoRunning.Remove(testCase.Identifier);
            _rhinoPending.Enqueue(entity);
        }

        // Events
        public override async Task OnConnectedAsync()
        {
            // setup
            var (address, port) = Context.GetAddress();
            var id = Context.ConnectionId;
            var model = new WorkerQueueModel
            {
                Address = address,
                ConnectionId = id,
                Created = DateTime.Now,
                GroupName = "RhinoWorkers",
                Port = port
            };

            // invoke
            _workers[id] = model;
            await Groups.AddToGroupAsync(id, "RhinoWorkers");

            // log
            Trace.TraceInformation($"Add-Worker -Connection {id} -Address {address} -Port {port} = OK");

            // base
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // setup
            var id = Context.ConnectionId;

            // invoke
            await Groups.RemoveFromGroupAsync(id, "RhinoWorkers");
            if (_workers.ContainsKey(id))
            {
                _workers.Remove(id);
            }

            // log
            Trace.TraceInformation($"Remove-Worker -Connection {id} = OK");

            // base
            await base.OnDisconnectedAsync(exception);
        }
    }
}
