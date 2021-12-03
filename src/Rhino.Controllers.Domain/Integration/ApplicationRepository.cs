/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using Microsoft.AspNetCore.Http;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Interfaces;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;

using System;
using System.Collections.Generic;

namespace Rhino.Controllers.Domain.Integration
{
    public class ApplicationRepository : IApplicationRepository
    {
        // members: state
        private readonly ILogger _logger;
        private readonly IEnumerable<Type> _types;

        /// <summary>
        /// Creates a new instance of ApplicationRepository.
        /// </summary>
        /// <param name="types">An IEnumerable<Type> implementation.</param>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        public ApplicationRepository(IEnumerable<Type> types, ILogger logger)
        {
            _types = types;
            _logger = logger;
        }

        /// <summary>
        /// Gets the connector configuration state of the repository.
        /// </summary>
        public RhinoConnectorConfiguration Configuration { get; private set; }

        /// <summary>
        /// Sets the connector configuration state of the repository.
        /// </summary>
        /// <param name="configuration">The configuration state to set.</param>
        /// <returns>Self reference.</returns>
        public IApplicationRepository SetConnector(RhinoConnectorConfiguration configuration)
        {
            // setup
            Configuration = configuration;

            // get
            return this;
        }

        #region *** Add    ***
        /// <summary>
        /// Add a new RhinoTestCase object into the target application.
        /// </summary>
        /// <param name="entity">The RhinoTestCase object to post.</param>
        /// <returns>The id of the RhinoTestCase.</returns>
        public string Add(RhinoTestCase entity)
        {
            // bad request
            if (Configuration == null)
            {
                return $"{StatusCodes.Status400BadRequest}";
            }

            // build
            var tempConfiguration = new RhinoConfiguration
            {
                ConnectorConfiguration = Configuration,
                TestsRepository = new[] { "-1" } // mocking test cases repository for connection validation.
            };
            var type = tempConfiguration.GetConnector(_types);

            // not found
            if (type == default)
            {
                return $"{StatusCodes.Status404NotFound}";
            }

            // build
            var connector = (IConnector)Activator.CreateInstance(type, new object[]
            {
                    tempConfiguration, _types, _logger, false
            });

            // get
            return connector.ProviderManager.CreateTestCase(entity);
        }
        #endregion

        #region *** Delete ***
        /// <summary>
        /// Deletes a RhinoTestCase from the target application.
        /// </summary>
        /// <param name="id">The RhinoTestCase id by which to delete.</param>
        /// <returns><see cref="int"/>.</returns>
        public int Delete(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes all RhinoTestCase from the target application.
        /// </summary>
        /// <returns><see cref="int"/>.</returns>
        public int Delete()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region *** Get    ***
        /// <summary>
        /// Gets all RhinoTestCase from the target application.
        /// </summary>
        /// <returns>A Collection of RhinoTestCase.</returns>
        public IEnumerable<RhinoTestCase> Get()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a RhinoTestCase from the target application.
        /// </summary>
        /// <param name="id">The RhinoTestCase id by which to get.</param>
        /// <returns><see cref="int"/> and RhinoTestCase object (if any).</returns>
        public (int StatusCode, RhinoTestCase Entity) Get(string id)
        {
            throw new NotImplementedException();
        }
        #endregion        

        #region *** Update ***
        /// <summary>
        /// Puts a new RhinoTestCase into the target application.
        /// </summary>
        /// <param name="id">The id of RhinoTestCase to put.</param>
        /// <param name="entity">The RhinoTestCase to update.</param>
        /// <returns><see cref="int"/> and the updated object (if any).</returns>
        public (int StatusCode, RhinoTestCase Entity) Update(string id, RhinoTestCase entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Puts a new RhinoTestCase into the target application.
        /// </summary>
        /// <param name="id">The id of RhinoTestCase to put.</param>
        /// <param name="fields">The RhinoTestCase to update.</param>
        /// <returns><see cref="int"/> and the updated object (if any).</returns>
        public (int StatusCode, RhinoTestCase Entity) Update(string id, IDictionary<string, object> fields)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}