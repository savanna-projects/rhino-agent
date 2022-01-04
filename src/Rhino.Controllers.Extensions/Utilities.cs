/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Api.Contracts;
using Rhino.Api.Contracts.AutomationProvider;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for <see cref="Stream"/> object and other related object.
    /// </summary>
    public static class Utilities
    {
        // members: state
        private static readonly IList<Assembly> s_assemblies = new List<Assembly>();

        /// <summary>
        /// Gets a distinct collection of <see cref="Type"/> loaded into the AppDomain.
        /// </summary>
        public static IList<Type> Types => DoGetTypes(string.Empty)
            .SelectMany(i => i.Types ?? Array.Empty<Type>())
            .Distinct()
            .ToList();

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

        #region *** Assemblies ***
        /// <summary>
        /// gets a collection of all assemblies where the executing assembly is currently located
        /// </summary>
        /// <returns>A collection of <see cref="Assembly"/>.</returns>
        public static IEnumerable<(Assembly Assembly, IEnumerable<Type> Types)> GetTypes()
        {
            return DoGetTypes(string.Empty);
        }

        /// <summary>
        /// gets a collection of all assemblies where the executing assembly is currently located
        /// </summary>
        /// <param name="root">The root folder from which to load the assemblies.</param>
        /// <returns>A collection of <see cref="Assembly"/>.</returns>
        public static IEnumerable<(Assembly Assembly, IEnumerable<Type> Types)> GetTypes(string root)
        {
            return DoGetTypes(root);
        }

        private static IEnumerable<(Assembly Assembly, IEnumerable<Type> Types)> DoGetTypes(string root)
        {
            // reset
            s_assemblies.Clear();

            // setup
            var mainLocation = string.IsNullOrEmpty(root)
                ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                : root;
            mainLocation ??= Environment.CurrentDirectory;
            var rootLocations = new[]
            {
                mainLocation
            };
            var pluginsLocation = new[]
            {
                Path.Combine(mainLocation, RhinoPluginEntry.PluginsGravityFolder),
                Path.Combine(mainLocation, "PluginsReporters"),
                Path.Combine(mainLocation, RhinoPluginEntry.PluginsConnectorsFolder)
            };
            var locations = pluginsLocation
                .Where(i => Directory.Exists(i))
                .SelectMany(i => Directory.GetDirectories(i))
                .Concat(rootLocations);

            // build files
            var files = locations
                .Where(i => Directory.Exists(i))
                .SelectMany(i => Directory.GetFiles(i))
                .Where(i => Regex.IsMatch(i, @"(?i)\.dll$"));

            // build
            foreach (var assemblyFile in files)
            {
                GetAssemblies(assemblyFile);
            }

            // get
            return s_assemblies.Select(i => GetPair(i)).Where(i => i.Assembly != null);
        }

        [SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used", Justification = "Must be loaded from file")]
        private static void GetAssemblies(string assemblyFile)
        {
            // load main assembly
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFile));
                assembly.GetTypes();
            }
            catch (FileNotFoundException)
            {
                assembly = Assembly.LoadFile(assemblyFile);
                try
                {
                    assembly.GetTypes();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.GetBaseException().Message);
                    return;
                }
            }
            catch (Exception e) when (e != null)
            {
                Console.WriteLine(e.GetBaseException().Message);
                return;
            }

            // build
            s_assemblies.Add(assembly);
            foreach (var item in assembly.GetReferencedAssemblies())
            {
                try
                {
                    var names = s_assemblies.Select(i => i.FullName).Any(i => i == item.FullName);
                    if (names)
                    {
                        continue;
                    }
                    var referenced = Assembly.Load(item);
                    GetAssemblies(referenced.Location);
                }
                catch (Exception e) when (e != null)
                {
                    Console.WriteLine(e.GetBaseException().Message);
                }
            }
        }

        private static (Assembly Assembly, IEnumerable<Type> Types) GetPair(Assembly assembly)
        {
            try
            {
                // setup
                var types = assembly.GetTypes();

                // get
                return (assembly, types);
            }
            catch (Exception e) when (e != null)
            {
                Console.WriteLine(e.GetBaseException().Message);
            }
            return (null, null);
        }
        #endregion
    }
}
