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
using Rhino.Controllers.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhino.Controllers.Domain.Automation
{
    /// <summary>
    /// Data Access Layer for Rhino API configurations repository.
    /// </summary>
    public class ConfigurationsRepository : Repository<RhinoConfiguration>
    {
        // constants
        private const string Name = "configurations";

        // members: state
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        /// <param name="liteDb">An ILiteDatabase implementation to use with the Repository.</param>
        /// <param name="configuration">An IConfiguration implementation to use with the Repository.</param>
        public ConfigurationsRepository(ILogger logger, ILiteDatabase liteDb, IConfiguration configuration)
            : base(logger, liteDb, configuration)
        {
            this.logger = logger.CreateChildLogger(nameof(ConfigurationsRepository));
        }

        #region *** Add    ***
        /// <summary>
        /// Add a new RhinoConfiguration object into the domain state.
        /// </summary>
        /// <param name="entity">The RhinoConfiguration object to post.</param>
        /// <returns>The id of the RhinoConfiguration.</returns>
        public override string Add(RhinoConfiguration entity)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // build           
            var entityModel = collection.AddEntityModel(entity);

            // sync
            var isUser = !string.IsNullOrEmpty(Authentication.UserName);
            var isPassword = !string.IsNullOrEmpty(Authentication.Password);
            entity.Authentication = isUser && isPassword ? Authentication : entity.Authentication;
            collection.UpdateEntityModel(entityModel.Id, entity);
            logger?.Debug($"Update-RhinoConfiguration -Id {entity.Id} = Ok");

            // results
            return $"{entity.Id}";
        }
        #endregion

        #region *** Delete ***
        /// <summary>
        /// Deletes a configuration from the domain state.
        /// </summary>
        /// <param name="id">The configuration id by which to delete.</param>
        /// <returns><see cref="int"/>.</returns>
        public override int Delete(string id)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // delete
            return LiteDb.GetCollection<RhinoEntityModel>(name).Delete<RhinoConfiguration>(id);
        }

        /// <summary>
        /// Deletes all configurations from the domain state.
        /// </summary>
        /// <returns><see cref="int"/>.</returns>
        public override int Delete()
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection > configuration
            return LiteDb.GetCollection<RhinoEntityModel>(name).Delete<RhinoConfiguration>();
        }
        #endregion

        #region *** Get    ***
        /// <summary>
        /// Gets all configuration in the domain collection.
        /// </summary>
        /// <returns>A Collection of RhinoConfiguration.</returns>
        public override IEnumerable<RhinoConfiguration> Get()
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // get configuration
            return collection.Get<RhinoConfiguration>(string.Empty).Entities;
        }

        /// <summary>
        /// Gets a configuration from the domain state.
        /// </summary>
        /// <param name="id">The configuration id by which to get.</param>
        /// <returns><see cref="int"/> and RhinoConfiguration object (if any).</returns>
        public override (int StatusCode, RhinoConfiguration Entity) Get(string id)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // get configuration
            var (statusCode, entities) = collection.Get<RhinoConfiguration>(id);

            // get
            return (statusCode, entities.FirstOrDefault());
        }
        #endregion        

        #region *** Update ***
        /// <summary>
        /// Put a RhinoConfiguration from the domain collection.
        /// </summary>
        /// <param name="id">The id of RhinoConfiguration to put.</param>
        /// <param name="entity">The RhinoConfiguration to update.</param>
        /// <returns><see cref="int"/> and the updated object (if any).</returns>
        public override (int StatusCode, RhinoConfiguration Entity) Update(string id, RhinoConfiguration entity)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // get configuration
            var (statusCode, configurations) = collection.Get<RhinoConfiguration>(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound || !configurations.Any())
            {
                logger?.Debug($"Update-Configuration -Id {id} = NotFound");
                return (statusCode, entity);
            }

            // build
            entity.Id = Guid.Parse(id);
            entity.Authentication = Authentication;

            // update
            collection.UpdateEntityModel(entity.Id, entity);

            // results
            return (StatusCodes.Status200OK, entity);
        }
        #endregion
    }
}