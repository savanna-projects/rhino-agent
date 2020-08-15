/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using LiteDB;
using Newtonsoft.Json;
using Gravity.Extensions;
using System;
using Microsoft.Extensions.DependencyInjection;
using Gravity.Services.DataContracts;

namespace Rhino.Agent.Domain
{
    /// <summary>
    /// Base Data Access Layer for Rhino API repositories.
    /// </summary>
    public abstract class Repository
    {
        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Domain.RhinoTestCaseRepository.</param>
        protected Repository(IServiceProvider provider)
        {
            LiteDb = provider.GetRequiredService<LiteDatabase>();
        }

        /// <summary>
        /// Gets or sets MongoDb collection name.
        /// </summary>
        public string Collection { get; internal set; }

        /// <summary>
        /// Gets the injected LiteDB.LiteDatabase instance for this Rhino.Agent.Domain.Repository.
        /// </summary>
        public LiteDatabase LiteDb { get; }

        // Generates MongoDb collection name.
        public static string GetCollectionName(Authentication  authentication , string prefix)
        {
            // serialize
            var stringBody = JsonConvert.SerializeObject(authentication).ToBase64();

            // convert
            return $"{prefix}_{stringBody}";
        }
    }
}