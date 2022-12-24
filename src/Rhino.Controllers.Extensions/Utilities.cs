/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Loader;

using Microsoft.AspNetCore.Http;

using Rhino.Api.Contracts;

using System.Collections.ObjectModel;

namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for <see cref="Stream"/> object and other related object.
    /// </summary>
    public static class Utilities
    {
        // state
        private static IList<Type> s_types;

        #region *** Types ***
        /// <summary>
        /// Gets a distinct collection of <see cref="Type"/> loaded into the AppDomain.
        /// </summary>
        public static IList<Type> Types
        {
            get
            {
                // already exists
                if (s_types?.Any() == true)
                {
                    return s_types;
                }

                // first time
                s_types = new AssembliesLoader().GetTypes().Distinct().ToList();

                // get
                return new ReadOnlyCollection<Type>(s_types);
            }
        }

        public static (int StatusCode, string Message) SyncAssemblies(params string[] locations)
        {
            try
            {
                locations ??= Array.Empty<string>();
                lock (s_types)
                {
                    s_types = new AssembliesLoader()
                        .GetTypes(".", locations)
                        .Distinct()
                        .ToList();
                }
            }
            catch (Exception e) when (e != null)
            {
                return (StatusCodes.Status500InternalServerError, e.GetBaseException().Message);
            }

            // get
            return (StatusCodes.Status204NoContent, string.Empty);
        }
        #endregion

        /// <summary>
        /// Gets the RhinoSpecification separator including the empty lines.
        /// </summary>
        public static string Separator
        {
            get
            {
                var doubleLine = Environment.NewLine + Environment.NewLine;
                return doubleLine + RhinoSpecification.Separator + doubleLine;
            }
        }
    }
}
