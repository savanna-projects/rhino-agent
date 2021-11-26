/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using LiteDB;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Domain.Extensions;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhino.Controllers.Domain.Automation
{
    /// <summary>
    /// Data Access Layer for Rhino API Test Cases repository.
    /// </summary>
    public class TestsRepository : Repository<RhinoTestCollection>, ITestsRepository
    {
        // constants
        private const string Name = "tests";

        // members: state
        private readonly ILogger _logger;
        private readonly IRepository<RhinoConfiguration> _configurationRepository;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        /// <param name="liteDb">An ILiteDatabase implementation to use with the Repository.</param>
        /// <param name="configuration">An IConfiguration implementation to use with the Repository.</param>
        /// <param name="configurationRepository">An IRepository<RhinoConfiguration> implementation to use with the Repository.</param>
        public TestsRepository(
            ILogger logger,
            ILiteDatabase liteDb,
            IConfiguration configuration,
            IRepository<RhinoConfiguration> configurationRepository) : base(logger, liteDb, configuration)
        {
            _logger = logger.CreateChildLogger(nameof(TestsRepository));
            _configurationRepository = configurationRepository;
        }

        #region *** Add    ***
        /// <summary>
        /// Add a new RhinoConfiguration object into the domain state.
        /// </summary>
        /// <param name="entity">The RhinoConfiguration object to post.</param>
        /// <returns>The id of the RhinoConfiguration.</returns>
        public override string Add(RhinoTestCollection entity)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // build           
            var entityModel = collection.AddEntityModel(entity);

            // sync
            collection.UpdateEntityModel(entityModel.Id, entity);
            _logger?.Debug($"Update-RhinoConfiguration -Id {entity.Id} = Ok");

            // cascade
            CascadeAdd(entity, entity.Configurations);

            // results
            return $"{entity.Id}";
        }

        private void CascadeAdd(RhinoTestCollection entity, IEnumerable<string> configurations)
        {
            foreach (var configuration in _configurationRepository.Get().Where(i => configurations.Contains($"{i.Id}")))
            {
                // add
                var testsRepository = configuration.TestsRepository.ToList();
                testsRepository.Add($"{entity.Id}");

                // apply
                configuration.TestsRepository = testsRepository;

                // update
                _configurationRepository.Update($"{configuration.Id}", configuration);
            }
        }
        #endregion

        #region *** Delete ***
        /// <summary>
        /// Deletes a test cases collection from the domain state.
        /// </summary>
        /// <param name="id">The configuration id by which to delete.</param>
        /// <returns><see cref="int"/>.</returns>
        public override int Delete(string id)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection > configuration
            var statusCode = LiteDb.GetCollection<RhinoEntityModel>(name).Delete<RhinoTestCollection>(id);
            CascadeDelete(id);

            // get
            return statusCode;
        }

        /// <summary>
        /// Deletes all configurations from the domain state.
        /// </summary>
        /// <returns><see cref="int"/>.</returns>
        public override int Delete()
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // build
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);
            var forDelete = collection.Get<RhinoTestCollection>(string.Empty).Entities.Select(i => $"{i.Id}").ToArray();
            var statusCode = LiteDb.GetCollection<RhinoEntityModel>(name).Delete<RhinoTestCollection>();

            // cascade
            foreach (var id in forDelete)
            {
                CascadeDelete(id);
            }

            // delete
            return statusCode;
        }

        private void CascadeDelete(string id)
        {
            foreach (var configuration in _configurationRepository.Get().Where(i => i.TestsRepository.Contains(id)))
            {
                // add
                var testsRepository = configuration.TestsRepository.ToList();
                testsRepository.Remove(id);

                // apply
                configuration.TestsRepository = testsRepository;

                // update
                _configurationRepository.Update($"{configuration.Id}", configuration);
            }
        }
        #endregion

        #region *** Get    ***
        /// <summary>
        /// Gets all test case collections in the domain.
        /// </summary>
        /// <returns>A Collection of RhinoTestCaseCollection.</returns>
        public override IEnumerable<RhinoTestCollection> Get()
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // get configuration
            return collection.Get<RhinoTestCollection>(string.Empty).Entities;
        }

        /// <summary>
        /// Gets a test case collection from the domain state.
        /// </summary>
        /// <param name="id">The RhinoTestCaseCollection id by which to get.</param>
        /// <returns><see cref="int"/> and RhinoTestCaseCollection object (if any).</returns>
        public override (int StatusCode, RhinoTestCollection Entity) Get(string id)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // get configuration
            var (statusCode, entities) = collection.Get<RhinoTestCollection>(id);

            // get
            return (statusCode, entities.FirstOrDefault());
        }
        #endregion 

        #region *** Update ***
        /// <summary>
        /// Update an existing RhinoTestCaseCollection in the domain collection.
        /// </summary>
        /// <param name="id">The id of RhinoTestCaseCollection to update.</param>
        /// <param name="entity">The RhinoTestCaseCollection to update.</param>
        /// <returns><see cref="int"/> and the updated object (if any).</returns>
        public override (int StatusCode, RhinoTestCollection Entity) Update(string id, RhinoTestCollection entity)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // get configuration
            var (statusCode, configurations) = collection.Get<RhinoTestCollection>(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound || !configurations.Any())
            {
                _logger?.Debug($"Update-RhinoTestCaseCollection -Id {id} = NotFound");
                return (statusCode, entity);
            }

            // build
            entity.Id = configurations.FirstOrDefault()?.Id ?? Guid.Empty;

            // update
            collection.UpdateEntityModel(entity.Id, entity);

            // results
            return (StatusCodes.Status200OK, entity);
        }

        /// <summary>
        /// Adds a <see cref="RhinoTestCaseDocument"/> into an existing collection under context.
        /// </summary>
        /// <param name="id">The collection id to patch into.</param>
        /// <param name="configuration">RhinoConfiguration.Id to update.</param>
        /// <returns><see cref="int"/> and the updated object (if any).</returns>
        public (int statusCode, RhinoTestCollection data) Update(string id, string configuration)
        {
            // setup
            var message = "Update-RhinoTestCaseCollection " +
                    $"-Id {id} " +
                    $"-Configuration {configuration} = ($(StatusCode), RhinoTestCaseCollection | RhinoConfiguration)";

            // exit conditions
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(configuration))
            {
                _logger?.Debug(message.Replace("$(StatusCode)", "BadRequest"));
                return (StatusCodes.Status400BadRequest, default);
            }

            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);
            var testCaseCollection = collection.Get<RhinoTestCollection>(id).Entities.FirstOrDefault();
            var (statusCode, onConfiguration) = _configurationRepository.SetAuthentication(Authentication).Get(configuration);

            // not found conditions
            if (testCaseCollection == default || statusCode == StatusCodes.Status404NotFound)
            {
                _logger?.Debug(message.Replace("$(StatusCode)", "NotFound"));
                return (StatusCodes.Status404NotFound, default);
            }

            // exit conditions
            if (testCaseCollection.Configurations.Contains(configuration))
            {
                _logger?.Debug(message.Replace("$(StatusCode)", "NoContent, Duplicate"));
                return (StatusCodes.Status204NoContent, testCaseCollection);
            }

            // update
            testCaseCollection.Configurations.Add(configuration);
            collection.UpdateEntityModel(Guid.Parse(id), entity: testCaseCollection);
            CascadeUpdate(testCaseCollection, onConfiguration);

            // get
            _logger?.Debug(message.Replace("$(StatusCode)", "Ok"));
            return (StatusCodes.Status204NoContent, testCaseCollection);
        }

        /// <summary>
        /// Adds a RhinoTestCaseModel into an existing RhinoTestCaseCollection in the domain.
        /// </summary>
        /// <param name="entity">The RhinoTestCaseModel entity to add.</param>
        /// <returns><see cref="int"/> and the updated object (if any).</returns>
        public (int statusCode, RhinoTestCollection data) Update(string id, RhinoTestModel entity)
        {
            // exit conditions
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(entity.Collection))
            {
                _logger?.Debug($"Update-RhinoTestCaseCollection -Id {id} = (BadRequest, RhinoTestCaseCollection | RhinoTestCaseModel)");
                return (StatusCodes.Status400BadRequest, default);
            }

            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);
            var testCaseCollection = collection.Get<RhinoTestCollection>(id).Entities.FirstOrDefault();

            // apply
            testCaseCollection.RhinoTestCaseModels.Add(entity);
            collection.UpdateEntityModel(Guid.Parse(id), entity: testCaseCollection);

            // response
            return (StatusCodes.Status200OK, testCaseCollection);
        }

        private void CascadeUpdate(RhinoTestCollection entity, RhinoConfiguration configuration)
        {
            //setup
            var id = $"{entity.Id}";

            // exit conditions
            if (configuration.TestsRepository.Contains(id))
            {
                return;
            }

            // build
            var testsRepository = configuration.TestsRepository.ToList();
            testsRepository.Add(id);
            configuration.TestsRepository = testsRepository;

            // update
            _configurationRepository.Update($"{configuration.Id}", configuration);
        }
        #endregion
    }
}
