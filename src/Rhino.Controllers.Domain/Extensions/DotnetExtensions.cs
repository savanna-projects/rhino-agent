/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet.Engine.Attributes;

using Microsoft.TeamFoundation;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Converters;
using Rhino.Api.Extensions;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;
using Rhino.Settings;

using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text.Json;

namespace Rhino.Controllers.Domain.Extensions
{
    internal static class DotnetExtensions
    {
        /// <summary>
        /// Converts an object to a DataTable based on the object public properties.
        /// </summary>
        /// <typeparam name="T">The object type to convert.</typeparam>
        /// <param name="objs">The object type.</param>
        /// <returns>The DataTable object.</returns>
        public static DataTable AddRows<T>(this DataTable table, IEnumerable<T> objs)
        {
            // bad request
            if (objs?.Any() == false)
            {
                return table;
            }

            // constants
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;

            // setup
            var properties = objs
                .First()
                .GetType()
                .GetProperties(Flags)
                .Where(i => i.GetGetMethod() != null);

            // create columns
            foreach (var property in properties)
            {
                var type = property.PropertyType.IsPrimitive
                    ? property.PropertyType
                    : typeof(string);
                table.Columns.Add(property.Name, type);
            }

            // iterate
            foreach (var obj in objs)
            {
                var row = table.NewRow();
                foreach (var property in properties)
                {
                    row[property.Name] = property.GetValue(obj);
                }
                table.Rows.Add(row);
            }

            // get
            return table;
        }

        public static bool DeepEqual<T>(this T source, T target, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            // not equal
            if (source.GetType() != target.GetType())
            {
                return false;
            }

            // setup
            string leftHand;
            string rightHand;
            var options = new JsonSerializerOptions();

            options.Converters.Add(new ExceptionConverter());
            options.Converters.Add(new MethodBaseConverter());
            options.Converters.Add(new TypeConverter());

            // string
            if ((source is string) && (target is string))
            {
                leftHand = source.ToString();
                rightHand = target.ToString();
            }
            else
            {
                leftHand = JsonSerializer.Serialize(source, options);
                rightHand = JsonSerializer.Serialize(target, options);
            }

            // sort
            leftHand = leftHand.Sort();
            rightHand = rightHand.Sort();

            // compare
            return leftHand.Equals(rightHand, comparison);
        }

        public static void TryUpdate<TValue>(this IDictionary<string, TValue> collection, string key, TValue value)
        {
            // bad request
            if (collection == null)
            {
                return;
            }
            if(!collection.ContainsKey(key))
            {
                return;
            }

            // update
            collection[key] = value;
        }
    }
}
