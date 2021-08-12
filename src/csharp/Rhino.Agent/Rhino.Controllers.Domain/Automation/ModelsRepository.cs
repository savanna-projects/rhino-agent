/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using LiteDB;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using Rhino.Api.Contracts.AutomationProvider;
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
    /// Data Access Layer for Rhino API Models repository.
    /// </summary>
    public class ModelsRepository : Repository<RhinoModelCollection>
    {
        // constants
        private const string Name = "models";

        // members: state
        private readonly ILogger logger;
        private readonly IRepository<RhinoConfiguration> configurationRepository;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        /// <param name="liteDb">An ILiteDatabase implementation to use with the Repository.</param>
        /// <param name="configuration">An IConfiguration implementation to use with the Repository.</param>
        /// <param name="configurationRepository">An IRepository<RhinoConfiguration> implementation to use with the Repository.</param>
        public ModelsRepository(
            ILogger logger,
            ILiteDatabase liteDb,
            IConfiguration configuration,
            IRepository<RhinoConfiguration> configurationRepository) : base(logger, liteDb, configuration)
        {
            this.logger = logger?.CreateChildLogger(nameof(ModelsRepository));
            this.configurationRepository = configurationRepository;
        }

        #region *** Add    ***
        /// <summary>
        /// Add a new RhinoPageModelsCollection object into the domain state.
        /// </summary>
        /// <param name="entity">The RhinoPageModes object to post.</param>
        /// <returns>The id of the RhinoPageModes.</returns>
        public override string Add(RhinoModelCollection entity)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // apply models
            entity.Models = GetModels(entity, collection);

            // exit conditions
            if (entity.Models.Count == 0)
            {
                return string.Empty;
            }

            // build
            var entityModel = collection.AddEntityModel(entity);

            // cascade
            if (entity.Configurations?.Count > 0)
            {
                CascadeAdd(entity);
            }

            // sync
            collection.UpdateEntityModel(entityModel.Id, entity);
            logger?.Debug($"Update-RhinoModelCollection -Id {entity.Id} = Ok");

            // results
            return $"{entity.Id}";
        }

        // gets a list of existing models after excluding existing ones exclude - models will not be overwritten
        private IList<RhinoPageModel> GetModels(RhinoModelCollection entity, ILiteCollection<RhinoEntityModel> collection)
        {
            try
            {
                // build - excluded
                var exludedNames = collection
                    .FindAll()
                    .Select(i => i.GetEntity<RhinoModelCollection>())
                    .SelectMany(i => i.Models)
                    .Select(i => i.Name)
                    .ToList();
                logger?.Debug($"Get-Models -Exists True = ${exludedNames.Count}");

                // build - included
                var models = entity.Models.Where(i => !exludedNames.Contains(i.Name)).ToList();
                logger?.Debug($"Get-Models -Exists False = ${models.Count}");

                // get
                return models;
            }
            catch (Exception e)
            {
                var baseException = e.GetBaseException();
                var message = $"Get-Models = (InternalServerError, {baseException.Message})";

                throw (Exception)Activator.CreateInstance(baseException.GetType(), new object[] { message });
            }
        }

        // add models collection to a configuration (cascade)
        private void CascadeAdd(RhinoModelCollection entity)
        {
            // load
            foreach (var configuration in configurationRepository.Get().ToList())
            {
                // setup
                var models = configuration.Models.ToList();

                // exit conditions
                if (models.Contains($"{entity.Id}"))
                {
                    continue;
                }

                // build
                models.Add($"{entity.Id}");
                configuration.Models = models;

                // update
                configurationRepository.Update($"{configuration.Id}", configuration);
                logger?.Debug($"Update-RhinoPageModes -Configuration {configuration} -Model {entity.Id} = Ok");
            }
        }
        #endregion

        #region *** Delete ***
        /// <summary>
        /// Deletes all models collections from the domain state.
        /// </summary>
        /// <returns><see cref="int"/>.</returns>
        public override int Delete()
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // delete
            foreach (var id in collection.FindAll().Select(i => $"{i.Id}").ToList())
            {
                DoDelete(id);
            }

            // get
            return StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Deletes a models collection from the domain state.
        /// </summary>
        /// <param name="id">The collection id by which to delete.</param>
        /// <returns><see cref="int"/>.</returns>
        public override int Delete(string id)
        {
            return DoDelete(id);
        }

        // executes delete routine
        private int DoDelete(string id)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection > models
            var entityModelCollection = LiteDb.GetCollection<RhinoEntityModel>(name);
            var configurations = configurationRepository.Get().Select(i => $"{i.Id}").ToList();
            var (statusCode, models) = entityModelCollection.Get<RhinoModelCollection>(id);
            var modelsCollection = models.FirstOrDefault();

            // not found
            if (statusCode == StatusCodes.Status404NotFound || modelsCollection == default)
            {
                logger?.Debug($"Delete-Model -Id {id} = NotFound");
                return statusCode;
            }

            // cascade
            foreach (var configuration in configurations)
            {
                CascadeDelete(configuration, modelsCollection);
            }

            // delete
            entityModelCollection.Delete(models.FirstOrDefault()?.Id);
            logger?.Debug($"Delete-Model -Id {id} = Ok");

            // get
            return StatusCodes.Status204NoContent;
        }

        // delete models collection from a configuration (cascade)
        private void CascadeDelete(string configuration, RhinoModelCollection modelsCollection)
        {
            // get collection
            var (statusCode, configurationEntity) = configurationRepository.Get(id: configuration);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return;
            }

            // get current models
            var configurationModels = configurationEntity.Models.ToList();

            // exit conditions
            if (!configurationModels.Contains($"{modelsCollection.Id}"))
            {
                return;
            }

            // delete models from configuration
            configurationModels.Remove($"{modelsCollection.Id}");

            // update
            configurationEntity.Models = configurationModels;
            configurationRepository.Update(id: configuration, configurationEntity);
        }
        #endregion

        #region *** Get    ***
        /// <summary>
        /// Gets all models collections in the domain state.
        /// </summary>
        /// <returns>A Collection of RhinoPageModes.</returns>
        public override IEnumerable<RhinoModelCollection> Get()
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // get configuration
            return collection.Get<RhinoModelCollection>(string.Empty).Entities;
        }

        /// <summary>
        /// Gets a models collection from the domain state.
        /// </summary>
        /// <param name="id">The configuration id by which to get.</param>
        /// <returns><see cref="int"/> and RhinoPageModes object (if any).</returns>
        public override (int StatusCode, RhinoModelCollection Entity) Get(string id)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // get configuration
            var (statusCode, entities) = collection.Get<RhinoModelCollection>(id);

            // get
            return (statusCode, entities.FirstOrDefault());
        }
        #endregion

        #region *** Update ***
        /// <summary>
        /// Puts a new RhinoPageModelsCollection into the domain collection.
        /// </summary>
        /// <param name="id">The id of RhinoPageModelsCollection to put.</param>
        /// <param name="entity">The RhinoPageModelsCollection to update.</param>
        /// <returns><see cref="int"/> and the updated entity (if any).</returns>
        public override (int StatusCode, RhinoModelCollection Entity) Update(string id, RhinoModelCollection entity)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // get configuration
            var (statusCode, configurations) = collection.Get<RhinoModelCollection>(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound || !configurations.Any())
            {
                logger?.Debug($"Update-PageModelsCollection -Id {id} = (NotFound, PageModelsCollection)");
                return (statusCode, entity);
            }

            // build
            entity.Id = configurations.FirstOrDefault()?.Id ?? default;

            // update
            collection.UpdateEntityModel(entity.Id, entity);

            // results
            return (StatusCodes.Status200OK, entity);
        }

        /// <summary>
        /// Puts a new RhinoPageModes into a RhinoPageModelsCollection collection.
        /// </summary>
        /// <param name="id">The id of RhinoPageModelsCollection to put.</param>
        /// <param name="configuration">The RhinoPageModes.Id to patch.</param>
        /// <returns><see cref="int"/> and the updated entity (if any).</returns>
        public (int StatusCode, RhinoModelCollection Entity) Update(string id, string configuration)
        {
            // exit conditions
            if (string.IsNullOrEmpty(configuration))
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // setup
            var name = SetCollectionName(Name).CollectionName;
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);
            var model = collection.Get<RhinoModelCollection>(id).Entities.FirstOrDefault();
            var (statusCode, _) = configurationRepository.Get(configuration);

            // not found conditions
            if (model == default || statusCode == StatusCodes.Status404NotFound)
            {
                logger?.Debug("Update-RhinoPageModelsCollection " +
                    $"-Id {id} " +
                    $"-Configuration {configuration} = (NotFound, RhinoPageModel || RhinoPageModes)");
                return (StatusCodes.Status404NotFound, default);
            }

            // update
            if (!model.Configurations.Contains(configuration))
            {
                model.Configurations.Add(configuration);
                collection.UpdateEntityModel(model.Id, model);
            }

            // response
            return (StatusCodes.Status200OK, model);
        }

        /// <summary>
        /// Puts a new RhinoPageModel into a RhinoPageModelsCollection collection.
        /// </summary>
        /// <param name="id">The id of the RhinoPageModelsCollection.</param>
        /// <param name="entity"><see cref="RhinoTestCaseDocument"/> data to create.</param>
        /// <returns><see cref="int"/> and the updated entity (if any).</returns>
        public (int StatusCode, RhinoModelCollection Entity) Patch(string id, RhinoPageModel entity)
        {
            // bad request
            if (string.IsNullOrEmpty(id))
            {
                logger?.Debug($"Update-RhinoModelCollection -Id {id} = (BadRequest, NoCollecction)");
                return (StatusCodes.Status400BadRequest, default);
            }

            // setup
            var name = SetCollectionName(Name).CollectionName;
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);
            var (statusCode, modelCollections) = collection.Get<RhinoModelCollection>(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound || !modelCollections.Any())
            {
                logger?.Debug($"Update-RhinoModelCollection -Id {id} = NotFound");
                return (StatusCodes.Status404NotFound, default);
            }

            // setup
            var modelCollection = modelCollections.FirstOrDefault();
            modelCollection.Models.Add(entity);

            // apply
            collection.UpdateEntityModel(Guid.Parse(id), entity: modelCollection);

            // response
            return (StatusCodes.Status200OK, modelCollection);
        }
        #endregion
    }
}