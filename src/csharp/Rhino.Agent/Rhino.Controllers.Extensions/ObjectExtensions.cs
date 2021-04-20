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

namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for <see cref="object"/>.
    /// </summary>
    public static class ObjectExtensions
    {
        // constants
        //private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

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
                IgnoreNullValues = true,
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

        //// TODO: fix all Gravity/Rhino attributes to exclude TypeId field.
        //// TODO: remove when attributes were fixed.
        ///// <summary>
        ///// Converts a non-serializable object into a serializable object.
        ///// </summary>
        ///// <typeparam name="T">The type of the object.</typeparam>
        ///// <param name="obj">The object con convert.</param>
        ///// <returns>A serializable object</returns>
        ///// <remarks>Supports only public properties. This is a bridge method and will be removed later on.</remarks>
        //public static object ToSerializable<T>(this T obj)
        //{
        //    // constants
        //    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;

        //    // setup
        //    var objOut = new Dictionary<string, object>();

        //    // build
        //    foreach (var property in obj.GetType().GetProperties(Flags).Where(i => !i.Name.Equals("TypeId", Compare)))
        //    {
        //        objOut[property.Name] = property.GetValue(obj);
        //    }

        //    // get
        //    return objOut;
        //}

        private static string DoToJson(object obj, JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize(obj, options);
        }
    }
}