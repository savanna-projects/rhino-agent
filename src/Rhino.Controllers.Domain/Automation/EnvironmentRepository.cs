/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Services.DataContracts;

using LiteDB;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using Rhino.Controllers.Domain.Extensions;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Models;

namespace Rhino.Controllers.Domain.Automation
{
    /// <summary>
    /// Data Access Layer for Rhino API environment repository.
    /// </summary>
    public class EnvironmentRepository : Repository<KeyValuePair<string, object>>, IEnvironmentRepository
    {
        // constants
        private const string Name = "environment";
        private const string EnvironmentName = "RhinoEnvironment";
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

        // members: state
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        /// <param name="liteDb">An ILiteDatabase implementation to use with the Repository.</param>
        /// <param name="configuration">An IConfiguration implementation to use with the Repository.</param>
        public EnvironmentRepository(ILogger logger, ILiteDatabase liteDb, IConfiguration configuration)
            : base(logger, liteDb, configuration)
        {
            _logger = logger.CreateChildLogger(nameof(EnvironmentRepository));
        }

        #region *** Add    ***
        /// <summary>
        /// Add a new parameters set to the domain state.
        /// </summary>
        /// <param name="entity">The environment object to post.</param>
        /// <returns>The id of the environment.</returns>
        public IDictionary<string, object> Add(IDictionary<string, object> entity)
        {
            // push to gravity
            foreach (var item in entity)
            {
                AutomationEnvironment.SessionParams[item.Key] = item.Value;
            }

            // invoke
            AddToCache(entity);

            // get
            return entity;
        }

        public IDictionary<string, object> Add(IDictionary<string, object> entity, bool encode)
        {
            // push to gravity
            foreach (var item in entity)
            {
                AutomationEnvironment.SessionParams[item.Key] = encode
                    ? $"{item.Value}".ConvertToBase64()
                    : item.Value;
            }

            // invoke
            AddToCache(entity);

            // get
            return entity;
        }

        /// <summary>
        /// Add a new parameter to the domain state.
        /// </summary>
        /// <param name="entity">The environment object to post.</param>
        /// <returns>The id of the environment.</returns>
        public override string Add(KeyValuePair<string, object> entity)
        {
            // push to gravity
            AutomationEnvironment.SessionParams[entity.Key] = entity.Value;

            // invoke
            AddToCache(new Dictionary<string, object>(new[] { entity }, StringComparer.OrdinalIgnoreCase));

            // get
            return $"{entity.Value}";
        }

        public string Add(KeyValuePair<string, object> entity, bool encode)
        {
            // push to gravity
            AutomationEnvironment.SessionParams[entity.Key] = encode
                ? $"{entity.Value}".ConvertToBase64()
                : entity.Value;

            // invoke
            AddToCache(new Dictionary<string, object>(new[] { entity }, StringComparer.OrdinalIgnoreCase));

            // get
            return $"{entity.Value}";
        }

        private void AddToCache(IDictionary<string, object> entity)
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // build           
            var environment = collection
                .Get<RhinoEnvironmentModel>(id: string.Empty)
                .Entities
                .FirstOrDefault(i => i.Name.Equals(EnvironmentName, Compare));

            // not found
            if (environment == default)
            {
                environment = new RhinoEnvironmentModel { Name = EnvironmentName };
                var entityModel = collection.AddEntityModel(environment);
                environment.Id = entityModel.Id;
            }

            // sync
            foreach (var item in entity)
            {
                environment.Environment[item.Key] = item.Value;
            }
            collection.UpdateEntityModel(environment.Id, environment);
            _logger?.Debug($"Update-RhinoEnvironmentModel -Id {environment.Id} = OK");
        }
        #endregion

        #region *** Delete ***
        /// <summary>
        /// Deletes a parameter from the domain state.
        /// </summary>
        /// <param name="id">The parameter id by which to delete.</param>
        /// <returns><see cref="int"/>.</returns>
        public override int Delete(string id)
        {
            _logger?.Debug($"Delete-Parameter -Id {id} = (NotImplemented, NotSuiteable)");
            return StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Deletes all parameters from the domain state.
        /// </summary>
        /// <returns><see cref="int"/>.</returns>
        public override int Delete()
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection > configuration
            return LiteDb.GetCollection<RhinoEntityModel>(name).Delete<RhinoEnvironmentModel>();
        }

        /// <summary>
        /// Deletes a parameter from the domain state.
        /// </summary>
        /// <param name="name">The parameter name to delete.</param>
        /// <returns><see cref="int"/>.</returns>
        public int DeleteByName(string name)
        {
            // build
            var environment = DoGet();

            // not found
            if (environment == default)
            {
                _logger?.Debug($"Delete-Parameter -Name {name} = (NotFound, NoEnvironment)");
                return StatusCodes.Status404NotFound;
            }

            // get parameter
            var isParameter = environment.Entity.ContainsKey(name);

            // not found
            if (!isParameter)
            {
                _logger?.Debug($"Delete-Parameter -Name {name} = (NotFound, NoParameter)");
                return StatusCodes.Status404NotFound;
            }

            // delete
            environment.Entity.Remove(key: name);
            environment.Collection.UpdateEntityModel(environment.Model.Id, environment.Model);

            // get
            return StatusCodes.Status204NoContent;
        }
        #endregion

        #region *** Get    ***
        /// <summary>
        /// Gets all parameters in the domain collection.
        /// </summary>
        /// <returns>A Collection of environment.</returns>
        public override IEnumerable<KeyValuePair<string, object>> Get()
        {
            return DoGet().Entity;
        }

        /// <summary>
        /// Gets an environment from the domain state.
        /// </summary>
        /// <param name="id">The environment id by which to get.</param>
        /// <returns><see cref="int"/> and KeyValuePair<string, object> object (if any).</returns>
        public override (int StatusCode, KeyValuePair<string, object> Entity) Get(string id)
        {
            _logger?.Debug($"Get-Parameter -Id {id} = (NotImplemented, NotSuiteable)");
            return (StatusCodes.Status200OK, new KeyValuePair<string, object>("key", "value"));
        }

        /// <summary>
        /// Gets a a parameter from the domain state.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns><see cref="int"/> and KeyValuePair<string, object> object (if any).</returns>
        public (int StatusCode, KeyValuePair<string, object> Entity) GetByName(string name)
        {
            // build           
            var environment = DoGet().Entity;

            // not found
            if (!environment.ContainsKey(name))
            {
                _logger?.Debug($"Get-Parameter -Name {name} = (NotFound, Environment | Parameter)");
                return (StatusCodes.Status404NotFound, new KeyValuePair<string, object>("", ""));
            }

            // get
            return (StatusCodes.Status200OK, new KeyValuePair<string, object>(name, environment[name]));
        }

        /// <summary>
        /// Sync RhinoEnvironment with Gravity AutomationEnvironment.
        /// </summary>
        /// <returns><see cref="int"/> and all environment parameters.</returns>
        public (int StatusCode, IDictionary<string, object> Entities) Sync()
        {
            return SyncFromCache(false);
        }

        /// <summary>
        /// Sync RhinoEnvironment with Gravity AutomationEnvironment.
        /// </summary>
        /// <returns><see cref="int"/> and all environment parameters.</returns>
        public (int StatusCode, IDictionary<string, object> Entities) Sync(bool encode)
        {
            return SyncFromCache(encode);
        }

        private (int StatusCode, IDictionary<string, object> Entities) SyncFromCache(bool encode)
        {
            // build           
            var environment = DoGet().Entity;

            // update
            foreach (var parameter in environment)
            {
                AutomationEnvironment.SessionParams[parameter.Key] = encode
                ? $"{parameter.Value}".ConvertToBase64()
                : parameter.Value;
            }
            _logger?.Debug($"Update-Parameter -Sync -All = (Ok, {environment.Count})");

            // get
            return (StatusCodes.Status200OK, environment);
        }
        #endregion

        #region *** Update ***
        /// <summary>
        /// Add a new parameter to the domain state.
        /// </summary>
        /// <param name="id">The id of the environment to add parameter to.</param>
        /// <param name="entity">The environment object to post.</param>
        /// <returns><see cref="int"/> and KeyValuePair<string, object> object (if any).</returns>
        public override (int StatusCode, KeyValuePair<string, object> Entity) Update(string id, KeyValuePair<string, object> entity)
        {
            _logger?.Debug("Update-Parameter" +
                $"-Id {id}" +
                $"-Key {entity.Key}" +
                $"-Value {entity.Value} = (NotImplemented, NotSuiteable)");

            return (StatusCodes.Status200OK, new KeyValuePair<string, object>("key", "value"));
        }
        #endregion

        // UTILITIES
        private (ILiteCollection<RhinoEntityModel> Collection, RhinoEnvironmentModel Model, IDictionary<string, object> Entity) DoGet()
        {
            // validate
            var name = SetCollectionName(Name).CollectionName;

            // get collection
            var collection = LiteDb.GetCollection<RhinoEntityModel>(name);

            // build           
            var model = collection
                .Get<RhinoEnvironmentModel>(id: string.Empty)
                .Entities
                .FirstOrDefault(i => i.Name.Equals(EnvironmentName, Compare));

            // get
            return (collection, model, model?.Environment ?? new Dictionary<string, object>());
        }
    }
}
