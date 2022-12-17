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
        private static IList<Type> s_typesCollection;
        private static IList<Type> s_cachedTypes;

        #region *** Types ***
        /// <summary>
        /// Gets a distinct collection of <see cref="Type"/> loaded into the AppDomain.
        /// </summary>
        public static IList<Type> Types
        {
            get
            {
                // data change
                if (s_typesCollection?.Any() == true && s_cachedTypes?.Any() == true)
                {
                    s_cachedTypes = s_typesCollection.Count > s_cachedTypes.Count
                        ? new ReadOnlyCollection<Type>(s_typesCollection)
                        : s_cachedTypes;
                }

                // no change (singleton)
                if (s_typesCollection?.Any() == false && s_cachedTypes?.Any() == true)
                {
                    return s_cachedTypes;
                }

                // first time
                var types = new AssembliesLoader().GetTypes().Distinct().ToList();

                // get
                return new ReadOnlyCollection<Type>(types);
            }
        }

        public static (int StatusCode, string Message) SyncAssemblies()
        {
            try
            {
                if (s_typesCollection == null)
                {
                    s_typesCollection = new AssembliesLoader()
                        .GetTypes()
                        .Distinct()
                        .ToList();
                }
                else
                {
                    lock (s_typesCollection)
                    {
                        s_typesCollection = new AssembliesLoader()
                            .GetTypes()
                            .Distinct()
                            .ToList();
                    }
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
