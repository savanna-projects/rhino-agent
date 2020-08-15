/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;
using LiteDB;

using Microsoft.Extensions.DependencyInjection;

using Rhino.Agent.Extensions;
using Rhino.Agent.Models;
using Rhino.Api.Contracts.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Rhino.Agent.Domain
{
    /// <summary>
    /// Data Access Layer for Rhino API test cases repository.
    /// </summary>
    public class RhinoTestCaseRepository : Repository
    {
        // members: state
        private readonly RhinoConfigurationRepository configurationRepository;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Domain.RhinoTestCaseRepository.</param>
        public RhinoTestCaseRepository(IServiceProvider provider)
            : base(provider)
        {
            configurationRepository = provider.GetRequiredService<RhinoConfigurationRepository>();
        }

        #region *** GET    ***
        /// <summary>
        /// Gets all RhinoTestCaseCollection under context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get RhinoTestCaseCollection.</param>
        /// <returns>A collection of RhinoTestCaseCollection.</returns>
        public (HttpStatusCode statusCode, IEnumerable<RhinoTestCaseCollection> data) Get(Authentication  authentication )
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoTestCaseCollection>(name: Collection);
            collection.EnsureIndex(i => i.Id);

            // get configuration
            return (HttpStatusCode.OK, collection.FindAll());
        }

        /// <summary>
        /// Gets a single RhinoTestCaseCollection from context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get RhinoTestCaseCollection.</param>
        /// <param name="id"><see cref="RhinoTestCaseCollection.Id"/> by which to find this RhinoTestCaseCollection.</param>
        /// <returns>A RhinoTestCaseCollection instance.</returns>
        public (HttpStatusCode statusCode, RhinoTestCaseCollection data) Get(Authentication  authentication , string id)
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoTestCaseCollection>(name: Collection);

            // get configuration
            return Get(id, collection);
        }
        #endregion

        #region *** POST   ***
        /// <summary>
        /// Creates a new RhinoTestCaseCollection under context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to create RhinoTestCaseCollection.</param>
        /// <param name="data">RhinoTestCaseCollection data to create.</param>
        /// <returns>The <see cref="RhinoTestCaseCollection.Id"/> of the newly created entity.</returns>
        public string Post(Authentication authentication, RhinoTestCaseCollection data)
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoTestCaseCollection>(name: Collection);

            // insert
            collection.Insert(entity: data);

            // exit conditions
            if (data.Configurations == null || data.Configurations.Count == 0)
            {
                return $"{data.Id}";
            }

            // cascade
            foreach (var configuration in data.Configurations)
            {
                ApplyToConfiguration(authentication, configuration, collection: data);
            }

            // response
            return $"{data.Id}";
        }
        #endregion

        #region *** PATCH  ***
        /// <summary>
        /// Adds a <see cref="RhinoTestCaseDocument"/> into an existing collection under context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to create <see cref="RhinoTestCaseDocument"/>.</param>
        /// <param name="id"><see cref="Collation.Id"/> to patch.</param>
        /// <param name="configuration">Rhino.Api.Contracts.Configuration.RhinoConfiguration.Id to patch.</param>
        /// <returns>The RhinoTestCaseCollection of the newly created entity.</returns>
        public (HttpStatusCode statusCode, RhinoTestCaseCollection data) Patch(Authentication authentication, string id, string configuration)
        {
            // exit conditions
            if (string.IsNullOrEmpty(configuration))
            {
                return (HttpStatusCode.BadRequest, default);
            }

            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoTestCaseCollection>(name: Collection);
            var onData = collection
                .FindAll()
                .FirstOrDefault(i => $"{i.Id}".Equals(id, StringComparison.OrdinalIgnoreCase));
            var (statusCode, data) = configurationRepository.Get(authentication, configuration);

            // not found conditions
            if (onData == default || statusCode == HttpStatusCode.NotFound.ToInt32())
            {
                return (HttpStatusCode.NotFound, default);
            }

            // apply
            if (!onData.Configurations.Contains(configuration))
            {
                onData.Configurations.Add(configuration);
                collection.Update(entity: onData);
                ApplyToConfiguration(authentication, configuration, collection: onData);
            }

            // response
            return (HttpStatusCode.NoContent, onData);
        }

        /// <summary>
        /// Adds a <see cref="RhinoTestCaseDocument"/> into an existing collection under context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to create <see cref="RhinoTestCaseDocument"/>.</param>
        /// <param name="data"><see cref="RhinoTestCaseDocument"/> data to create.</param>
        /// <returns>The RhinoTestCaseCollection of the newly created entity.</returns>
        public (HttpStatusCode statusCode, RhinoTestCaseCollection data) Patch(Authentication  authentication , RhinoTestCaseDocument data)
        {
            // exit conditions
            if(string.IsNullOrEmpty(data.Collection))
            {
                return (HttpStatusCode.BadRequest, default);
            }

            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoTestCaseCollection>(name: Collection);
            var onData = collection
                .FindAll()
                .FirstOrDefault(i => $"{i.Id}".Equals(data.Collection, StringComparison.OrdinalIgnoreCase));

            // apply
            onData.RhinoTestCaseDocuments.Add(data);
            collection.Update(entity: onData);

            // response
            return (HttpStatusCode.OK, onData);
        }

        /// <summary>
        /// Updates an existing RhinoTestCaseCollection entity.
        /// </summary>
        /// <param name="authentication">Authentication object by which to update RhinoTestCaseCollection.</param>
        /// <param name="data">RhinoTestCaseCollection data to update.</param>
        /// <returns>Updated RhinoTestCaseCollection entity.</returns>
        public (HttpStatusCode statusCode, RhinoTestCaseCollection data) Patch(Authentication  authentication , RhinoTestCaseCollection data)
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoTestCaseCollection>(name: Collection);
            var onData = collection
                .FindAll()
                .FirstOrDefault(i => $"{i.Id}".Equals($"{data.Id}", StringComparison.OrdinalIgnoreCase));

            // exit conditions
            if (onData == default)
            {
                return (HttpStatusCode.NotFound, default);
            }

            // update
            onData.Configurations = data.Configurations;
            onData.RhinoTestCaseDocuments = data.RhinoTestCaseDocuments;
            foreach (var configuration in onData.Configurations)
            {
                ApplyToConfiguration(authentication, configuration, onData);
            }
            collection.Update(onData);

            // response
            return (HttpStatusCode.OK, onData);
        }
        #endregion

        #region *** DELETE ***
        /// <summary>
        /// DELETE a collection from this domain state.
        /// </summary>
        /// <param name="authentication">Authentication object by which to update RhinoTestCaseCollection.</param>
        /// <param name="id">The collection id by which to DELETE.</param>
        /// <returns>Status code.</returns>
        public HttpStatusCode Delete(Authentication  authentication , string id)
        {
            return DoDelete(authentication, id);
        }

        /// <summary>
        /// DELETE all collections from this domain state.
        /// </summary>
        /// <param name="authentication">Authentication object by which to update RhinoTestCaseCollection.</param>
        /// <returns>Status code.</returns>
        public void Delete(Authentication  authentication )
        {
            // validate
            CreateCollection(authentication);

            // get collection > configuration
            var collection = LiteDb.GetCollection<RhinoTestCaseCollection>(name: Collection);

            // delete
            foreach (var id in collection.FindAll().Select(i => $"{i.Id}"))
            {
                DoDelete(authentication, id);
            }
        }

        private HttpStatusCode DoDelete(Authentication  authentication , string id)
        {
            // validate
            CreateCollection(authentication);

            // get collection > configuration
            var collection = LiteDb.GetCollection<RhinoTestCaseCollection>(name: Collection);
            var configurations = LiteDb.GetCollection<RhinoConfiguration>().FindAll().Select(i => $"{i.Id}");
            var (statusCode, data) = Get(id, collection);

            // not found
            if (statusCode == HttpStatusCode.NotFound)
            {
                return statusCode;
            }

            // cascade
            foreach (var configuration in configurations)
            {
                RemoveFromConfiguration(authentication, configuration, collection: data);
            }

            // delete
            collection.Delete(data.Id);
            return HttpStatusCode.OK;
        }
        #endregion

        // UTILITIES
        // TODO: move to extensions (on Repository)
        /// <summary>
        /// Creates a collection based on the user details provided for this instance.
        /// </summary>
        /// <param name="authentication">Authentication object by which to access the collection.</param>
        public void CreateCollection(Authentication  authentication )
        {
            Collection = GetCollectionName(authentication, prefix: "collection");
        }

        // TODO: merge with RemoveFromConfiguration
        // apply a collection to all assigned configurations (cascade)
        private void ApplyToConfiguration(Authentication  authentication , string configuration, RhinoTestCaseCollection collection)
        {
            // validate
            var name = GetCollectionName(authentication, prefix: "configuration");

            // get collection
            var onCollection = LiteDb.GetCollection<RhinoConfiguration>(name);
            var onConfiguration = onCollection
                .FindAll()
                .FirstOrDefault(i => $"{i.Id}".Equals(configuration, StringComparison.OrdinalIgnoreCase));

            // exit conditions
            if (onConfiguration == default)
            {
                return;
            }

            // apply
            var tests = onConfiguration.TestsRepository.ToList();
            if (!tests.Contains($"{collection.Id}"))
            {
                tests.Add($"{collection.Id}");
            }
            onConfiguration.TestsRepository = tests.ToArray();

            // update
            onCollection.Update(onConfiguration);
        }

        // TODO: merge with ApplyToConfiguration
        // apply a collection to all assigned configurations (cascade)
        private void RemoveFromConfiguration(Authentication  authentication , string configuration, RhinoTestCaseCollection collection)
        {
            // validate
            var name = GetCollectionName(authentication, prefix: "configuration");

            // get collection
            var onCollection = LiteDb.GetCollection<RhinoConfiguration>(name);
            var onConfiguration = onCollection
                .FindAll()
                .FirstOrDefault(i => $"{i.Id}".Equals(configuration, StringComparison.OrdinalIgnoreCase));

            // exit conditions
            if (onConfiguration == default)
            {
                return;
            }

            // apply
            var tests = onConfiguration.TestsRepository.ToList();
            if (tests.Contains($"{collection.Id}"))
            {
                tests.Remove($"{collection.Id}");
            }
            onConfiguration.TestsRepository = tests.ToArray();

            // update
            onCollection.Update(onConfiguration);
        }

        // gets a configuration by id
        private (HttpStatusCode statusCode, RhinoTestCaseCollection data) Get(string id, ILiteCollection<RhinoTestCaseCollection> collection)
        {
            // set index            
            collection.EnsureIndex(i => i.Id);

            // get
            var data = collection
                .FindAll()
                .ToList()
                .Find(i => $"{i.Id}".Equals(id, StringComparison.OrdinalIgnoreCase));

            // not found
            return data == default ? (HttpStatusCode.NotFound, default) : (HttpStatusCode.OK, data);
        }
    }
}