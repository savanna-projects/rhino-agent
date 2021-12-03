/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for <see cref="object"/>.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Serialize and <see cref="object"/> to Json.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to serialize.</param>
        /// <returns>Json representation of the <see cref="object"/>.</returns>
        public static string ToJson(this object obj)
        {
            // setup
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };

            // get
            return DoToJson(obj, options);
        }

        /// <summary>
        /// Serialize and <see cref="object"/> to Json.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to serialize.</param>
        /// <returns>Json representation of the <see cref="object"/>.</returns>
        public static string ToJson(this object obj, JsonSerializerOptions options)
        {
            return DoToJson(obj, options);
        }

        private static string DoToJson(object obj, JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize(obj, options);
        }
    }
}
