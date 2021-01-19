using Gravity.Services.DataContracts;

using LiteDB;

using Rhino.Agent.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Rhino.Agent.Domain
{
    /// <summary>
    /// Data Access Layer for Rhino API environments repository.
    /// </summary>
    public class RhinoEnvironmentRepository : Repository
    {
        public RhinoEnvironmentRepository(IServiceProvider provider)
            : base(provider)
        { }


        public void Test(Authentication authentication)
        {
            // ensure
            CreateCollection(authentication);

            // get environment
            var collection = LiteDb.GetCollection<RhinoEnvironmentModel>(name: Collection);
            var environmentCollection = Get(name: Collection, collection);

            // not found
            if (environmentCollection.StatusCode == HttpStatusCode.NotFound)
            {
                var onEnvironment = new RhinoEnvironmentModel
                {
                    Environment = new ConcurrentDictionary<string, object>(),
                    Name = Collection
                };

                collection.Insert(onEnvironment);
            }

            if (1 + 1 == 2)
            {
                var a = Get(name: Collection, collection);
            }
        }

        public (HttpStatusCode StatusCode, RhinoEnvironmentModel Model) Get(Authentication authentication)
        {
            try
            {
                // ensure
                CreateCollection(authentication);

                // get environment
                var collection = LiteDb.GetCollection<RhinoEnvironmentModel>(name: Collection);
                var environmentCollection = Get(name: Collection, collection);

                // not found
                if (environmentCollection.StatusCode == HttpStatusCode.NotFound)
                {
                    var onEnvironment = new RhinoEnvironmentModel
                    {
                        Environment = new ConcurrentDictionary<string, object>(),
                        Name = Collection
                    };

                    return (HttpStatusCode.OK, onEnvironment);
                }

                // append
                var entity = Get(name: Collection, collection).Environment;

                // save
                return (HttpStatusCode.OK, entity);
            }
            catch (Exception e) when (e != null)
            {
                return (HttpStatusCode.InternalServerError, new RhinoEnvironmentModel());
            }
        }

        public HttpStatusCode Put(Authentication authentication, string name, string value)
        {
            try
            {
                // ensure
                CreateCollection(authentication);

                // get environment
                var collection = LiteDb.GetCollection<RhinoEnvironmentModel>(name: Collection);
                var environmentCollection = Get(name: Collection, collection);

                // not found
                if (environmentCollection.StatusCode == HttpStatusCode.NotFound)
                {
                    var onEnvironment = new RhinoEnvironmentModel
                    {
                        Environment = new ConcurrentDictionary<string, object>(),
                        Name = Collection
                    };

                    collection.Insert(onEnvironment);
                }

                // append
                var entity = Get(name: Collection, collection).Environment;
                entity.Environment[name] = value;

                // save
                collection.Update(entity);
                return HttpStatusCode.OK;
            }
            catch (Exception e) when (e != null)
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode Sync(Authentication authentication)
        {
            try
            {
                // ensure
                CreateCollection(authentication);

                // get environment
                var collection = LiteDb.GetCollection<RhinoEnvironmentModel>(name: Collection);
                var environmentCollection = Get(name: Collection, collection);

                // not found
                if (environmentCollection.StatusCode == HttpStatusCode.NotFound)
                {
                    return HttpStatusCode.NotFound;
                }

                // get
                var entity = Get(name: Collection, collection).Environment;
                entity.Environment ??= new ConcurrentDictionary<string, object>();

                // sync
                foreach (var item in entity.Environment)
                {
                    AutomationEnvironment.SessionParams[item.Key] = item.Value;
                }

                // save
                return HttpStatusCode.OK;
            }
            catch (Exception e) when (e != null)
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode Delete(Authentication authentication, string name)
        {
            return DoDelete(authentication, name);
        }

        public HttpStatusCode Delete(Authentication authentication)
        {
            return DoDelete(authentication, "Delete-Entity -Type RhinoEnvironmentModel");
        }

        private HttpStatusCode DoDelete(Authentication authentication, string name)
        {
            try
            {
                // ensure
                CreateCollection(authentication);

                // get environment
                var collection = LiteDb.GetCollection<RhinoEnvironmentModel>(name: Collection);
                var environmentCollection = Get(name: Collection, collection);

                // not found
                if (environmentCollection.StatusCode == HttpStatusCode.NotFound)
                {
                    return HttpStatusCode.NoContent;
                }

                // get
                var entity = Get(name: Collection, collection).Environment;
                entity.Environment ??= new ConcurrentDictionary<string, object>();

                // delete
                if(name.Equals("Delete-Entity -Type RhinoEnvironmentModel", StringComparison.OrdinalIgnoreCase))
                {
                    collection.Delete(entity.Id);
                }
                else if (entity.Environment.ContainsKey(name))
                {
                    entity.Environment.Remove(name);
                    collection.Update(entity);
                }

                // save
                return HttpStatusCode.NoContent;
            }
            catch (Exception e) when (e != null)
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Creates a collection based on the user details provided for this instance.
        /// </summary>
        /// <param name="authentication">Authentication object by which to access the collection.</param>
        public void CreateCollection(Authentication authentication)
        {
            Collection = GetCollectionName(authentication, prefix: "environment");
        }

        // gets an environment by id
        private static (HttpStatusCode StatusCode, RhinoEnvironmentModel Environment) Get(
            string name,
            ILiteCollection<RhinoEnvironmentModel> collection)
        {
            // set index            
            collection.EnsureIndex(i => i.Name);

            // get
            var environment = collection
                .FindAll()
                .ToList()
                .Find(i => $"{i.Name}".Equals(name, StringComparison.OrdinalIgnoreCase));

            var a = collection.FindAll().ToList();

            // not found
            if (environment == default)
            {
                return (HttpStatusCode.NotFound, default);
            }

            // delete
            return (HttpStatusCode.OK, environment);
        }
    }
}