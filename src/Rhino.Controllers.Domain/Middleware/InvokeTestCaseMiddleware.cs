/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Microsoft.AspNetCore.SignalR.Client;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Engine;
using Rhino.Connectors.Text;

using System.Diagnostics;

namespace Rhino.Controllers.Domain.Middleware
{
    /// <summary>
    /// Middleware for getting a RhinoTestCase from the active Rhino queue, invoke it and return results.
    /// </summary>
    public class InvokeTestCaseMiddleware
    {
        // members
        private readonly RhinoAutomationEngine _engine;
        private readonly HubConnection _connection;

        /// <summary>
        /// Initialize a new instance of InvokeTestCaseMiddleware object.
        /// </summary>
        /// <param name="connection">HubConnection to use with the middleware.</param>
        /// <param name="configuration">RhinoConfiguration to use with the middleware.</param>
        public InvokeTestCaseMiddleware(HubConnection connection, RhinoConfiguration configuration)
        {
            // setup
            var provider = new TextAutomationProvider(configuration);

            // build
            _connection = connection;
            _engine = new RhinoAutomationEngine(provider);
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="testCase">RhinoTestCase to invoke.</param>
        /// <param name="context">TestContext data to use with the RhinoTestCase.</param>
        /// <param name="callbackRoute">The Hub callback route to return the RhinoTestCase when invocation is complete.</param>
        public Task Invoke(RhinoTestCase testCase, IDictionary<string, object> context, string callbackRoute)
        {
            // setup
            testCase.Context = context;
            RhinoTestCase testCaseResult;

            // invoke
            try
            {
                testCaseResult = _engine.Invoke(testCase);
            }
            catch (Exception e) when (e != null)
            {
                Trace.TraceError($"{e}");
                return _connection.InvokeAsync("repair", testCase, testCase.Context);
            }

            // callback
            testCaseResult ??= testCase;
            return _connection.InvokeAsync(callbackRoute, testCaseResult, testCaseResult.Context);
        }
    }
}
