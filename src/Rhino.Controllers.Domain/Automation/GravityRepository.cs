/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Services.Comet;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Interfaces;
using Rhino.Connectors.Text;
using Rhino.Controllers.Domain.Interfaces;

namespace Rhino.Controllers.Domain.Automation
{
    public class GravityRepository : IGravityRepository
    {
        // members: state
        private readonly Orbit _client;
        private readonly TextConnector _textConnector;
        private readonly ILogger _logger;
        private readonly IEnumerable<Type> _types;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="client">Gravity client implementation to use with the Repository.</param>
        /// <param name="textConnector">TextConnector to use with the Repository.</param>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        /// <param name="types">A collection of types to use with the repository.</param>
        public GravityRepository(Orbit client, TextConnector textConnector, ILogger logger, IEnumerable<Type> types)
        {
            _client = client;
            _textConnector = textConnector;
            _logger = logger?.CreateChildLogger(nameof(GravityRepository));
            _types = types;
        }

        #region *** Invoke  ***
        /// <summary>
        /// Invokes a WebAutomation object.
        /// </summary>
        /// <param name="automation">The  WebAutomation to invoke.</param>
        public (int StatusCode, OrbitResponse Response) Invoke(WebAutomation automation)
        {
            try
            {
                // invoke
                var response = _client.RunAutomation(automation);

                // get
                return (StatusCodes.Status200OK, response);
            }
            catch (Exception e) when (e != null)
            {
                // log
                var fatal = $"Invoke-WebAutomation = (InternalServerError | {e.GetBaseException().Message})";
                _logger?.Fatal(fatal, e.GetBaseException());

                // get
                return (StatusCodes.Status500InternalServerError, default);
            }
        }

        /// <summary>
        /// Invokes a WebAutomation object.
        /// </summary>
        /// <param name="automation">The  WebAutomation to invoke.</param>
        public Task<OrbitResponse> InvokeAsync(WebAutomation automation)
        {
            return _client.RunAutomationAsync(automation);
        }
        #endregion

        #region *** Convert ***
        /// <summary>
        /// Converts RhinoStep (specifications) to an ActionRule object.
        /// </summary>
        /// <param name="specifications">The specifications to convert.</param>
        /// <returns>An ActionRule object converted from the specifications.</returns>
        public (int StatusCode, ActionRule ActionRule) Convert(string specifications)
        {
            return Convert(Array.Empty<ExternalRepository>(), specifications);
        }

        /// <summary>
        /// Converts RhinoStep (specifications) to an ActionRule object.
        /// </summary>
        /// <param name="specifications">The specifications to convert.</param>
        /// <param name="repositories">A collection of ExternalRepository to retrieve action from.</param>
        /// <returns>An ActionRule object converted from the specifications.</returns>
        public (int StatusCode, ActionRule ActionRule) Convert(string specifications, IEnumerable<ExternalRepository> repositories)
        {
            return Convert(repositories, specifications);
        }

        private (int StatusCode, ActionRule ActionRule) Convert(IEnumerable<ExternalRepository> repositories, string specifications)
        {
            // setup
            var testCase = GetTestCase(specifications);
            var repository = new[]
            {
                testCase.ToString()
            };

            // build
            _textConnector.Configuration.TestsRepository = repository;
            _textConnector.Configuration.ExternalRepositories= repositories;
            _textConnector.ProviderManager.Configuration.TestsRepository = repository;
            _textConnector.ProviderManager.Configuration.ExternalRepositories = repositories;

            // connect
            var connector = _textConnector.Connect();
            var actionRule = GetActionRule(connector);

            // get
            return GetActionRule(connector);
        }

        private static RhinoTestCase GetTestCase(string specifications)
        {
            // setup
            var step = new RhinoTestStep
            {
                Action = specifications
            };

            // get
            return new RhinoTestCase
            {
                Scenario = "N/A",
                Steps = new[] { step }
            };
        }

        private static (int StatusCode, ActionRule ActionRule) GetActionRule(IConnector connector)
        {
            // setup
            var testCase = connector?.ProviderManager?.TestRun?.TestCases?.FirstOrDefault();

            // bad request
            if(testCase == default)
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // get
            var actionRule = testCase.Steps?.FirstOrDefault()?.ActionRule;

            // bad request
            if (actionRule == default)
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // get
            return (StatusCodes.Status200OK, actionRule);
        }
        #endregion
    }
}
