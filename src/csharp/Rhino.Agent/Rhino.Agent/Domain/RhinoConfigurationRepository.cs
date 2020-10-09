/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;
using LiteDB;

using Rhino.Api.Contracts.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Rhino.Agent.Domain
{
    /// <summary>
    /// Data Access Layer for Rhino API configurations repository.
    /// </summary>
    public class RhinoConfigurationRepository : Repository
    {
        public RhinoConfigurationRepository(IServiceProvider provider)
            : base(provider)
        { }

        #region *** DELETE ***
        /// <summary>
        /// DELETE a configuration from this domain state.
        /// </summary>
        /// <param name="authentication">Authentication object by which to access the collection.</param>
        /// <param name="id">The configuration id by which to DELETE.</param>
        /// <returns>Status code.</returns>
        public HttpStatusCode Delete(Authentication authentication, string id)
        {
            // validate
            CreateCollection(authentication);

            // get collection > configuration
            var collection = LiteDb.GetCollection<RhinoConfiguration>(name: Collection);
            var (statusCode, configuration) = Get(id, collection);

            // not found
            if (statusCode == HttpStatusCode.NotFound)
            {
                return HttpStatusCode.NotFound;
            }

            // delete
            collection.Delete(configuration.Id);
            return HttpStatusCode.NoContent;
        }

        /// <summary>
        /// DELETE all configurations from this domain state.
        /// </summary>
        /// <param name="authentication">Authentication object by which to access the collection.</param>
        /// <returns>Status code.</returns>
        public HttpStatusCode Delete(Authentication  authentication)
        {
            // validate
            CreateCollection(authentication);

            // get collection > configuration
            var collection = LiteDb.GetCollection<RhinoConfiguration>(name: Collection);

            // delete
            collection.DeleteAll();
            return HttpStatusCode.NoContent;
        }
        #endregion

        #region *** GET    ***
        /// <summary>
        /// GET a configuration from this domain state.
        /// </summary>
        /// <param name="authentication">Authentication object by which to access the collection.</param>
        /// <param name="id">The configuration id by which to GET.</param>
        /// <returns>Status code and configuration (if any).</returns>
        public (HttpStatusCode statusCode, RhinoConfiguration data) Get(Authentication  authentication , string id)
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoConfiguration>(name: Collection);

            // get configuration
            return Get(id, collection);
        }

        /// <summary>
        /// GET all configuration in this domain collection.
        /// </summary>
        /// <param name="authentication">Authentication object by which to access the collection.</param>
        /// <returns>Collection of Rhino.Api.Contracts.Configuration.RhinoConfiguration.</returns>
        public IEnumerable<RhinoConfiguration> Get(Authentication  authentication )
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoConfiguration>(name: Collection);
            collection.EnsureIndex(i => i.Id);

            // get configuration
            return collection.FindAll();
        }
        #endregion

        #region *** POST   ***
        /// <summary>
        /// POST a new Rhino.Api.Contracts.Configuration.RhinoConfiguration into this domain collection.
        /// </summary>
        /// <param name="authentication">Authentication object by which to access the collection.</param>
        /// <param name="data">The Rhino.Api.Contracts.Configuration.RhinoConfiguration to POST.</param>
        /// <returns>Rhino.Api.Contracts.Configuration.RhinoConfiguration.Id.</returns>
        public string Post(Authentication  authentication , RhinoConfiguration data)
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoConfiguration>(name: Collection);

            // security
            data.Authentication = authentication;

            // insert
            collection.Insert(entity: data);

            // results
            return $"{data.Id}";
        }
        #endregion

        #region *** PUT    ***
        /// <summary>
        /// PUT a new Rhino.Api.Contracts.Configuration.RhinoConfiguration into this domain collection.
        /// </summary>
        /// <param name="authentication">Authentication object by which to access the collection.</param>
        /// <param name="id">Rhino.Api.Contracts.Configuration.RhinoConfiguration.Id to PUT.</param>
        /// <param name="data">The Rhino.Api.Contracts.Configuration.RhinoConfiguration to PUT.</param>
        /// <returns>Status code and configuration (if any).</returns>
        public (HttpStatusCode statusCode, RhinoConfiguration data) Put(Authentication  authentication , string id, RhinoConfiguration data)
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoConfiguration>(name: Collection);

            // get configuration
            var (statusCode, configuration) = Get(id, collection);

            // not found
            if (statusCode == HttpStatusCode.NotFound)
            {
                return (statusCode, configuration);
            }

            // update
            data.Id = configuration.Id;
            data.Authentication = authentication;
            collection.Update(entity: data);

            // results
            return (HttpStatusCode.OK, data);
        }
        #endregion

        /// <summary>
        /// Creates a collection based on the user details provided for this instance.
        /// </summary>
        /// <param name="authentication">Authentication object by which to access the collection.</param>
        public void CreateCollection(Authentication  authentication)
        {
            Collection = GetCollectionName(authentication, prefix: "configuration");
        }

        // gets a configuration by id
        private static (HttpStatusCode statusCode, RhinoConfiguration configuration) Get(string id, ILiteCollection<RhinoConfiguration> collection)
        {
            // set index            
            collection.EnsureIndex(i => i.Id);

            // get
            var configuration = collection
                .FindAll()
                .ToList()
                .Find(i => $"{i.Id}".Equals(id, StringComparison.OrdinalIgnoreCase));

            // not found
            if (configuration == default)
            {
                return (HttpStatusCode.NotFound, default);
            }

            // delete
            return (HttpStatusCode.OK, configuration);
        }
    }
}