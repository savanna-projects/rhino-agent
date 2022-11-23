/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Microsoft.AspNetCore.Http;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Contracts.Events;
using Rhino.Api.Extensions;
using Rhino.Api.Interfaces;
using Rhino.Controllers.Domain.Extensions;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Models;

using System.Collections.Concurrent;

namespace Rhino.Controllers.Domain.Orchestrator
{
    public class HubRepository : IHubRepository
    {
        // members: injection
        private readonly AppSettings _appSettings;
        private readonly ConcurrentQueue<RhinoTestRun> _completed;
        private readonly ConcurrentQueue<TestCaseQueueModel> _pending;
        private readonly IDictionary<string, TestCaseQueueModel> _running;
        private readonly IDictionary<string, RhinoTestRun> _testRuns;

        /// <summary>
        /// Initialize a new instance of HubRepository object.
        /// </summary>
        /// <param name="pending">An implementation for pending RhinoTestCase object.</param>
        /// <param name="running">An implementation for running RhinoTestCase object.</param>
        public HubRepository(
            AppSettings appSettings,
            ConcurrentQueue<RhinoTestRun> completed,
            ConcurrentQueue<TestCaseQueueModel> pending,
            IDictionary<string, TestCaseQueueModel> running,
            IDictionary<string, RhinoTestRun> testRuns)
        {
            _appSettings = appSettings;
            _completed = completed;
            _pending = pending;
            _running = running;
            _testRuns = testRuns;
        }

        /// <summary>
        /// Creates an asynchronous `RhinoTestRun` entity.
        /// </summary>
        /// <param name="configuration">The configuration to create the run by.</param>
        /// <returns>The run creation status.</returns>
        public (int StatusCode, object Entity) CreateTestRun(RhinoConfiguration configuration)
        {
            // setup
            var connector = configuration.GetConnector();
            var id = string.Empty;
            var isRunning = false;

            // setup: delegates
            connector.TestEnqueuing += TestSetup;   // intercept connector queue and pull the test
            connector.RunInvoked += RunTeardown;    // cleanup relevant queues at the end of the run
            connector.RunInvoking += (sender, e) => // gets the run id from the underline connector and set in collection
            {
                id = e.TestRun.Key;
                isRunning = true;
                _testRuns[e.TestRun.Key] = e.TestRun;
            };

            // start an invocation session
            connector.InvokeAsync();

            // wait for creation
            var timeout = DateTime.Now.AddMinutes(_appSettings.Hub.CreationTimeout);
            while (!isRunning && DateTime.Now < timeout)
            {
                Task.Delay(2000).Wait();
            }

            // get
            return string.IsNullOrEmpty(id)
                ? (StatusCodes.Status500InternalServerError, new { Id = "N/A" })
                : (StatusCodes.Status200OK, new { Id = id });
        }

        private void RunTeardown(object sender, TestRunInvocationEventArgs e)
        {
            _testRuns.Remove(e.TestRun.Key);
            _completed.Enqueue(e.TestRun);
        }

        private void TestSetup(object sender, ConnectorEventArgs e)
        {
            // setup
            var connector = (IConnector)sender;
            var testCase = connector.RequestTest();

            // set
            _pending.Enqueue(new()
            {
                Connector = connector,
                RegisterationTime = DateTime.Now,
                TestCase = testCase,
                Worker = default
            });
        }

        /// <summary>
        /// Gets the asynchronous status of all runs.
        /// </summary>
        /// <returns>The asynchronous status of the run.</returns>
        public (int StatusCode, RunsStatusModel Entity) GetStatus()
        {
            // setup
            var pending = _pending
                .AsEnumerable()
                .Select(i => i.Connector.ProviderManager.TestRun.Key);
            var running = _running.Select(i => i.Value.Connector.ProviderManager.TestRun.Key);
            var runs = pending.Concat(running).Where(i => !string.IsNullOrEmpty(i)).Distinct();

            // build
            var entity = new RunsStatusModel
            {
                TotalPending = pending.Count(),
                TotalRunning = running.Count(),
                TotalRuns = runs.Count(),
                Runs = runs
            };

            // get
            return (StatusCodes.Status200OK, entity);
        }

        /// <summary>
        /// Gets the asynchronous status of a run.
        /// </summary>
        /// <param name="id">The run id.</param>
        /// <returns>The asynchronous status of the run.</returns>
        public (int StatusCode, RunStatusModel Entity) GetStatus(string id)
        {
            // constants
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // setup
            var pending = _pending
                .AsEnumerable()
                .Where(i => i.Connector.GetRunKey().Equals(id, Compare));
            var running = _running.Where(i => i.Value.Connector.GetRunKey().Equals(id, Compare));

            // not found
            if (!_testRuns.ContainsKey(id))
            {
                return (StatusCodes.Status404NotFound, new RunStatusModel());
            }

            // setup
            var run = _testRuns[id];
            var totalTests = run.TestCases.Count();
            var totalPending = pending.Count();
            var totalRunning = running.Count();
            var completed = (double)(run.TestCases.Count() - totalPending - totalRunning);
            var progress = completed == 0 ? 0 : completed / totalTests * 100;

            // build
            var entity = new RunStatusModel
            {
                Completed = (int)completed,
                Id = run.Key,
                Pending = pending.Select(i => $"{i.TestCase.Scenario}:{i.TestCase.Iteration}"),
                Progress = Math.Round(progress, 2),
                RunningTime = DateTime.Now.Subtract(run.Start),
                StartTime = run.Start,
                Total = totalTests,
                TotalPending = totalPending,
                TotalRunning = totalRunning,
                Running = running.Select(i => new
                {
                    i.Value.TestCase.Key,
                    i.Value.TestCase.Identifier,
                    i.Value.TestCase.Scenario,
                    i.Value.TestCase.Iteration,
                    i.Value.TestCase.DataSource,
                    i.Value.Worker
                })
            };

            // get
            return (StatusCodes.Status200OK, entity);
        }

        /// <summary>
        /// Removes all the asynchronous runs from the server state.
        /// </summary>
        public void Reset()
        {
            _running.Clear();
            _testRuns.Clear();
            _pending.Clear();
        }
    }
}
