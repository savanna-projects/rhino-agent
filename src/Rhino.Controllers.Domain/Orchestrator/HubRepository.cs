/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.TeamFoundation.Build.WebApi;

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
        // members
        private IConnector _connector;

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
            _connector = configuration.GetConnector();
            var id = string.Empty;
            var isRunning = false;

            // setup: delegates
            _connector.TestEnqueuing += TestSetup;   // intercept connector queue and pull the test
            _connector.RunInvoked += RunTeardown;    // cleanup relevant queues at the end of the run
            _connector.RunInvoking += (sender, e) => // gets the run id from the underline connector and set in collection
            {
                id = e.TestRun.Key;
                isRunning = true;
                _testRuns[e.TestRun.Key] = e.TestRun;
            };

            // start an invocation session
            _connector.InvokeAsync();

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
            // remove from test runs queue
            _testRuns.Remove(e.TestRun.Key);

            // enforce number of completed in queue
            var maxCompleted = _appSettings.Hub.MaxCompleted == 0 ? 1 : _appSettings.Hub.MaxCompleted;
            _completed.Enqueue(e.TestRun);

            // clean
            while (_completed.Count > maxCompleted)
            {
                _ = _completed.TryDequeue(out _);
            }
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
        /// Gets a RhinoTestRun from the completed list.
        /// </summary>
        /// <param name="id">The run id.</param>
        /// <returns>The RhinoTestRun (null if not found) and the status code.</returns>
        public (int StatusCode, IEnumerable<string> Entities) GetCompleted()
        {
            // extract
            var runs = _completed.Select(i => i.Key);

            // get
            return runs == default
                ? (StatusCodes.Status404NotFound, Array.Empty<string>())
                : (StatusCodes.Status200OK, runs);
        }

        /// <summary>
        /// Gets a RhinoTestRun from the completed list.
        /// </summary>
        /// <param name="id">The run id.</param>
        /// <returns>The RhinoTestRun (null if not found) and the status code.</returns>
        public (int StatusCode, RhinoTestRun Entity) GetCompleted(string id)
        {
            // extract
            var run = _completed.FirstOrDefault(i => i.Key.Equals(id, StringComparison.OrdinalIgnoreCase));

            // get
            return run == default
                ? (StatusCodes.Status404NotFound, default)
                : (StatusCodes.Status200OK, run);
        }

        /// <summary>
        /// Gets a RunStatusModel from the running list.
        /// </summary>
        /// <param name="id">The RhinoTestCase id (not key).</param>
        /// <returns>The RunStatusModel (null if not found) and the status code.</returns>
        public (int StatusCode, TestCaseQueueModel Entity) GetRunningTest(string id)
        {
            // extract
            var isTest = _running.TryGetValue(id, out TestCaseQueueModel testOut);
            var test = isTest ? testOut : default;

            // get
            return test == default
                ? (StatusCodes.Status404NotFound, default)
                : (StatusCodes.Status200OK, test);
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
