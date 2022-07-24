/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Extensions;
using Gravity.Services.DataContracts;

using LiteDB;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;

namespace Rhino.Controllers.Domain
{
    /// <summary>
    /// Base Data Access Layer for Rhino API repositories.
    /// </summary>
    public abstract class Repository<T> : IRepository<T>
    {
        // constants
        public readonly string DataEncryptionConfiguration = "Rhino:StateManager:DataEncryptionKey";

        // members: state
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        /// <param name="liteDb">An ILiteDatabase implementation to use with the Repository.</param>
        /// <param name="configuration">An IConfiguration implementation to use with the Repository.</param>
        protected Repository(ILogger logger, ILiteDatabase liteDb, IConfiguration configuration)
        {
            LiteDb = liteDb;
            Configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Gets or sets MongoDb collection name.
        /// </summary>
        public string CollectionName { get; private set; }

        /// <summary>
        /// Gets the last saved Authentication information.
        /// </summary>
        public Authentication Authentication { get; private set; } = new Authentication
        {
            Password = string.Empty,
            Username = string.Empty
        };

        /// <summary>
        /// Gets the injected LiteDB.LiteDatabase instance for this Rhino.Agent.Domain.Repository.
        /// </summary>
        public ILiteDatabase LiteDb { get; }

        /// <summary>
        /// Gets the appSettings.json used by the repository.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Sets the authentication information.
        /// </summary>
        /// <param name="authentication">The authentication information.</param>
        /// <returns>Self reference to the creator repository.</returns>
        public IRepository<T> SetAuthentication(Authentication authentication)
        {
            // setup
            Authentication = authentication;

            // log
            var username = authentication.Username.Length > 3
                ? authentication.Username[..3]
                : authentication.Username;
            _logger?.Debug($"Set-Authentication -User {username}*** -Password HaHa ;o) = Ok");

            // get
            return this;
        }

        /// <summary>
        /// Creates a collection based on the user details provided for this instance.
        /// </summary>
        /// <param name="name">The collection name.</param>
        public IRepository<T> SetCollectionName(string name)
        {
            // serialize
            var stringBody = JsonConvert.SerializeObject(Authentication).ToBase64();
            var enStringBody = stringBody.Encrypt(Configuration.GetValue(DataEncryptionConfiguration, string.Empty)).RemoveNonWord();

            // convert
            CollectionName = $"{name}_{enStringBody}";
            _logger?.Debug($"Set-CollectionName -Name: {CollectionName} = Ok");

            // get
            return this;
        }

        #region *** CRUD ***
        /// <summary>
        /// Adds one new entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The ID of the new entity.</returns>
        public abstract string Add(T entity);

        /// <summary>
        /// Delete all entities.
        /// </summary>
        /// <returns><see cref="int"/> code of the operation.</returns>
        public abstract int Delete();

        /// <summary>
        /// Delete one entity.
        /// </summary>
        /// <param name="id">The entity id to delete.</param>
        /// <returns><see cref="int"/> code of the operation.</returns>
        public abstract int Delete(string id);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>A collection of entities.</returns>
        public abstract IEnumerable<T> Get();

        /// <summary>
        /// Get one entity.
        /// </summary>
        /// <param name="id">The id of the entity to retrieve.</param>
        /// <returns><see cref="int"/> code of the operation and the entity (if found).</returns>
        public abstract (int StatusCode, T Entity) Get(string id);

        /// <summary>
        /// Update an entity.
        /// </summary>
        /// <param name="id">The id of the entity to update.</param>
        /// <param name="entity">The update payload.</param>
        /// <returns><see cref="int"/> code of the operation and the updated entity.</returns>
        public abstract (int StatusCode, T Entity) Update(string id, T entity);

        /// <summary>
        /// Update an entity.
        /// </summary>
        /// <param name="id">The id of the entity to update.</param>
        /// <param name="fields">The update payload.</param>
        /// <returns><see cref="int"/> code of the operation and the updated entity.</returns>
        public virtual (int StatusCode, T Entity) Update(string id, IDictionary<string, object> fields)
        {
            // log
            _logger?.Debug($"Update-Entity -Partial -Type {typeof(T).Name} = NotImplemented");

            // get
            return (StatusCodes.Status501NotImplemented, default);
        }
        #endregion
    }
}