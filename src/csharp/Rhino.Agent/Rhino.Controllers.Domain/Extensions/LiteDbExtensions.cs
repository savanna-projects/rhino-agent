/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using LiteDB;

using Microsoft.AspNetCore.Http;

using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Rhino.Controllers.Domain.Extensions
{
    /// <summary>
    /// Extension package for LiteDb.
    /// </summary>
    internal static class LiteDbExtensions
    {
        // constants
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

        // members: state
        private static readonly ILogger logger = ControllerUtilities.GetLogger(typeof(LiteDbExtensions));

        #region *** Add    ***
        /// <summary>
        /// Add a new entity into a given collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity to add.</typeparam>
        /// <param name="collection">The collection to add the entity to.</param>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The created entity model.</returns>
        public static RhinoEntityModel AddEntityModel<T>(this ILiteCollection<RhinoEntityModel> collection, T entity)
        {
            // logger
            var type = typeof(T).Name;

            // validate settable GUID id
            var isGuid = typeof(T).GetProperty("Id").PropertyType == typeof(Guid);
            var isSettable = isGuid && typeof(T).GetProperty("Id").SetMethod != null;

            if (!isSettable)
            {
                logger?.Debug($"Add-EntityModel -Type {type} = (BadRequst, NotSettable)");
                return default;
            }

            // build
            var entityModel = new RhinoEntityModel();

            // insert
            collection.Insert(entityModel);
            logger?.Debug($"Add-EntityModel -Type {type} = {entityModel.Id}");

            // sync id
            entity.GetType().GetProperty("Id").SetValue(entity, entityModel.Id);

            // get
            return entityModel;
        }
        #endregion

        #region *** Delete ***
        /// <summary>
        /// Deletes an entity from the domain state.
        /// </summary>
        /// <param name="collection">The collection to add the entity to.</param>
        /// <param name="id">The entity id by which to delete.</param>
        /// <returns><see cref="int"/>.</returns>
        public static int Delete<T>(this ILiteCollection<RhinoEntityModel> collection, string id)
        {
            // logger
            var entityType = typeof(T).Name;

            // exit conditions
            var isGuid = Guid.TryParse(id, out Guid idOut);
            if (!isGuid)
            {
                logger?.Debug($"Delete-{entityType} -Id {id} = (BadRequest, NotGuid)");
                return StatusCodes.Status400BadRequest;
            }

            // setup
            var (statusCode, entity) = collection.Get<T>(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound || !entity.Any())
            {
                logger?.Debug($"Delete-{entityType} -Id {id} = NotFound");
                return StatusCodes.Status404NotFound;
            }

            // delete
            collection.Delete(idOut);
            logger?.Debug($"Delete-{entityType} -Id {id} = Ok, NoContent");

            // get
            return StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Deletes all entities from the domain state.
        /// </summary>
        /// <returns><see cref="int"/>.</returns>
        public static int Delete<T>(this ILiteCollection<RhinoEntityModel> collection)
        {
            // logger
            var entityType = typeof(T).Name;

            // setup
            collection.EnsureIndex(i => i.Id, unique: true);

            // delete
            collection.DeleteAll();
            logger?.Debug($"Delete-{entityType} -All True = (Ok, NoContent)");

            // get
            return StatusCodes.Status204NoContent;
        }
        #endregion

        #region *** Get    ***
        /// <summary>
        /// Gets entities from a given collection.
        /// </summary>
        /// <typeparam name="T">The entities target <see cref="Type"/>.</typeparam>
        /// <param name="collection">The collection from which to return the entities.</param>
        /// <param name="id">The id of the entity to find (pass null or empty to get all entities).</param>
        /// <returns><see cref="int"/> and a collection of entities or an empty collection if not entities found.</returns>
        public static (int StatusCode, IEnumerable<T> Entities) Get<T>(this ILiteCollection<RhinoEntityModel> collection, string id)
        {
            return OnGet<T>(collection, id, (_, collection) => collection.EnsureIndex(i => i.Id, unique: true));
        }

        /// <summary>
        /// Gets entities from a given collection.
        /// </summary>
        /// <typeparam name="T">The entities target <see cref="Type"/>.</typeparam>
        /// <param name="collection">The collection from which to return the entities.</param>
        /// <param name="id">The id of the entity to find (pass null or empty to get all entities).</param>
        /// <param name="onGet">A lambda expression to middle before the get action occurs.</param>
        /// <returns><see cref="int"/> and a collection of entities or an empty collection if not entities found.</returns>
        public static (int StatusCode, IEnumerable<T> Entities) Get<T>(
            this ILiteCollection<RhinoEntityModel> collection,
            string id,
            Action<string, ILiteCollection<RhinoEntityModel>> onGet)
        {
            return OnGet<T>(collection, id, onGet);
        }
        #endregion

        #region *** Update ***
        /// <summary>
        /// Update a new entity in a given collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity to update.</typeparam>
        /// <param name="collection">The collection to add the entity to.</param>
        /// <param name="id">The ID of the entity to update.</param>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The updated entity model.</returns>
        public static RhinoEntityModel UpdateEntityModel<T>(this ILiteCollection<RhinoEntityModel> collection, Guid id, T entity)
        {
            // logger
            var type = typeof(T).Name;

            // setup
            var entityModel = new RhinoEntityModel
            {
                Id = id,
                Entity = entity.ToJson()
            };

            // update
            collection.Update(entityModel);
            logger?.Debug($"Update-EntityModel -Type {type} -Id {id} = Ok");

            // get
            return entityModel;
        }
        #endregion

        private static (int StatusCode, IEnumerable<T> Entities) OnGet<T>(
            ILiteCollection<RhinoEntityModel> collection,
            string id,
            Action<string, ILiteCollection<RhinoEntityModel>> onGet)
        {
            // logger
            var entityType = typeof(T).Name;

            // user plugin
            onGet(id, collection);

            // all
            if (string.IsNullOrEmpty(id))
            {
                var entities = collection.FindAll().Select(i => i.GetEntity<T>()).ToArray();
                var result = (StatusCode: StatusCodes.Status200OK, Entities: entities);
                logger?.Debug($"Get-{entityType} = {result.Entities.Length}");

                return result;
            }

            // single
            var entity = collection.FindAll().ToList().Find(i => $"{i.Id}".Equals(id, Compare));

            // not found
            if (entity == default)
            {
                logger?.Debug($"Get-{entityType} -Id {id} = NotFound");
                return (StatusCodes.Status404NotFound, Array.Empty<T>());
            }

            // get
            logger?.Debug($"Get-{entityType} -Id {id} = Ok");
            return (StatusCodes.Status200OK, new[] { entity.GetEntity<T>() });
        }
    }
}