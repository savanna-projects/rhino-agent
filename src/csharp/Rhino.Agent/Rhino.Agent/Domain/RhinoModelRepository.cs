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
    /// Data Access Layer for Rhino API Models repository.
    /// </summary>
    public class RhinoModelRepository : Repository
    {
        // members: state
        private readonly RhinoConfigurationRepository configurationRepository;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Domain.RhinoTestCaseRepository.</param>
        public RhinoModelRepository(IServiceProvider provider)
            : base(provider)
        {
            configurationRepository = provider.GetRequiredService<RhinoConfigurationRepository>();
        }

        #region *** GET    ***
        /// <summary>
        /// Gets all RhinoPageModelCollection under context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get RhinoPageModelCollection.</param>
        /// <returns>A collection of RhinoPageModelCollection.</returns>
        public (HttpStatusCode statusCode, IEnumerable<RhinoPageModelCollection> data) Get(Authentication  authentication )
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoPageModelCollection>(name: Collection);
            collection.EnsureIndex(i => i.Id);

            // get configuration
            return (HttpStatusCode.OK, collection.FindAll());
        }

        /// <summary>
        /// Gets a single RhinoPageModelCollection from context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get RhinoPageModelCollection.</param>
        /// <param name="id"><see cref="RhinoPageModelCollection.Id"/> by which to find this RhinoPageModelCollection.</param>
        /// <returns>A RhinoPageModelCollection instance.</returns>
        public (HttpStatusCode statusCode, RhinoPageModelCollection data) Get(Authentication  authentication , string id)
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoPageModelCollection>(name: Collection);

            // get configuration
            return Get(id, collection);
        }
        #endregion

        #region *** POST   ***
        /// <summary>
        /// Creates a new RhinoPageModelCollection under context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to create RhinoPageModelCollection.</param>
        /// <param name="data">RhinoPageModelCollection data to create.</param>
        /// <returns>The <see cref="RhinoPageModelCollection.Id"/> of the newly created entity.</returns>
        public string Post(Authentication  authentication , RhinoPageModelCollection data)
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoPageModelCollection>(name: Collection);

            // get models
            var namesToExclude = collection.FindAll().SelectMany(i => i.Models).Select(i => i.Name);
            data.Models = data.Models.Where(i => !namesToExclude.Contains(i.Name)).ToList();

            // insert
            if (data.Models.Count == 0)
            {
                return string.Empty;
            }
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
        /// <returns>The RhinoPageModelCollection of the newly created entity.</returns>
        public (HttpStatusCode statusCode, RhinoPageModelCollection data) Patch(Authentication  authentication , string id, string configuration)
        {
            // exit conditions
            if (string.IsNullOrEmpty(configuration))
            {
                return (HttpStatusCode.BadRequest, default);
            }

            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoPageModelCollection>(name: Collection);
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
        /// <returns>The RhinoPageModelCollection of the newly created entity.</returns>
        public (HttpStatusCode statusCode, RhinoPageModelCollection data) Patch(Authentication  authentication , RhinoTestCaseDocument data)
        {
            // exit conditions
            if (string.IsNullOrEmpty(data.Collection))
            {
                return (HttpStatusCode.BadRequest, default);
            }

            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoPageModelCollection>(name: Collection);
            var onData = collection
                .FindAll()
                .FirstOrDefault(i => $"{i.Id}".Equals(data.Collection, StringComparison.OrdinalIgnoreCase));

            // apply
            collection.Update(entity: onData);

            // response
            return (HttpStatusCode.OK, onData);
        }

        /// <summary>
        /// Updates an existing RhinoPageModelCollection entity.
        /// </summary>
        /// <param name="authentication">Authentication object by which to update RhinoPageModelCollection.</param>
        /// <param name="data">RhinoPageModelCollection data to update.</param>
        /// <returns>Updated RhinoPageModelCollection entity.</returns>
        public (HttpStatusCode statusCode, RhinoPageModelCollection data) Patch(Authentication  authentication , RhinoPageModelCollection data)
        {
            // validate
            CreateCollection(authentication);

            // get collection
            var collection = LiteDb.GetCollection<RhinoPageModelCollection>(name: Collection);
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
            onData.Models = data.Models;
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
        /// <param name="authentication">Authentication object by which to update RhinoPageModelCollection.</param>
        /// <param name="id">The collection id by which to DELETE.</param>
        /// <returns>Status code.</returns>
        public HttpStatusCode Delete(Authentication  authentication , string id)
        {
            return DoDelete(authentication, id);
        }

        /// <summary>
        /// DELETE all collections from this domain state.
        /// </summary>
        /// <param name="authentication">Authentication object by which to update RhinoPageModelCollection.</param>
        /// <returns>Status code.</returns>
        public void Delete(Authentication  authentication )
        {
            // validate
            CreateCollection(authentication);

            // get collection > configuration
            var collection = LiteDb.GetCollection<RhinoPageModelCollection>(name: Collection);

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
            var collection = LiteDb.GetCollection<RhinoPageModelCollection>(name: Collection);
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
        public void CreateCollection(Authentication  authentication)
        {
            Collection = GetCollectionName(authentication, prefix: "models");
        }

        // TODO: merge with RemoveFromConfiguration
        // apply a collection to all assigned configurations (cascade)
        private void ApplyToConfiguration(Authentication  authentication , string configuration, RhinoPageModelCollection collection)
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
            var elements = onConfiguration.Models.ToList();
            if (!elements.Contains($"{collection.Id}"))
            {
                elements.Add($"{collection.Id}");
            }
            onConfiguration.Models = elements.ToArray();

            // update
            onCollection.Update(onConfiguration);
        }

        // TODO: merge with ApplyToConfiguration
        // apply a collection to all assigned configurations (cascade)
        private void RemoveFromConfiguration(Authentication  authentication , string configuration, RhinoPageModelCollection collection)
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
        private (HttpStatusCode statusCode, RhinoPageModelCollection data) Get(string id, ILiteCollection<RhinoPageModelCollection> collection)
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