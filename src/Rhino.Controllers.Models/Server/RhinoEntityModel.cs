/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Abstraction for saving models state inside LiteDb.
    /// Will prevent data loss due to different serializations.
    /// </summary>
    [DataContract]
    public class RhinoEntityModel
    {
        // members: state
        private readonly static JsonSerializerOptions jsonSettings = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// Gets or sets the ID of the model.
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Json representation of the model.
        /// </summary>
        [DataMember]
        public string Entity { get; set; }

        /// <summary>
        /// Parses the text representing a single JSON value into an instance of the type
        /// </summary>
        /// <typeparam name="T">The target type of the JSON value.</typeparam>
        /// <returns>A TValue representation of the JSON value.</returns>
        public T GetEntity<T>()
        {
            return DoGetEntity<T>(jsonSettings);
        }

        /// <summary>
        /// Parses the text representing a single JSON value into an instance of the type
        /// </summary>
        /// <typeparam name="T">The target type of the JSON value.</typeparam>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <returns>A TValue representation of the JSON value.</returns>
        public T GetEntity<T>(JsonSerializerOptions options)
        {
            return DoGetEntity<T>(options);
        }

        private T DoGetEntity<T>(JsonSerializerOptions options)
        {
            // setup
            Entity = string.IsNullOrEmpty(Entity) ? "{}" : Entity;

            // get
            return JsonSerializer.Deserialize<T>(Entity, options);
        }
    }
}