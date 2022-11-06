/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Data;
using System.Reflection;

namespace Rhino.Controllers.Domain.Extensions
{
    internal static class CsharpExtensions
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
    }
}
