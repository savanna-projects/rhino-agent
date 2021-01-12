/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rhino.Api.Contracts.AutomationProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Rhino.Agent.Extensions
{
    /// <summary>
    /// Internal Utilities package.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Normalize driver parameters to match Gravity's driver parameters contract.
        /// </summary>
        /// <param name="driverParameters">Driver parameters to normalize.</param>
        /// <returns>Normalized driver parameters.</returns>
        public static IEnumerable<IDictionary<string, object>> ParseDriverParameters(IEnumerable<IDictionary<string, object>> driverParameters)
        {
            // setup
            var onDriverParameters = new List<IDictionary<string, object>>();

            // iterate
            foreach (var item in driverParameters)
            {
                var driverParam = item;
                if (driverParam.ContainsKey(ContextEntry.Capabilities))
                {
                    var capabilitiesBody = ((JObject)driverParam[ContextEntry.Capabilities]).ToString();
                    driverParam[ContextEntry.Capabilities] =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(capabilitiesBody);
                }
                onDriverParameters.Add(driverParam);
            }

            // results
            return onDriverParameters;
        }

        /// <summary>
        /// Gets a list of available reports created by automation runs.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> by which to fetch settings.</param>
        /// <returns>A list of reports.</returns>
        public static IEnumerable<(string Path, string Name)> GetReports(IConfiguration configuration)
        {
            // setup
            var path = DoGetStaticReportsFolder(configuration);

            // get
            return Directory.GetDirectories(path).Select(i => (i, Path.GetFileName(i)));
        }

        /// <summary>
        /// Gets the static reports folder in which static reports can be served.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> by which to fetch settings.</param>
        /// <returns>Static reports folder.</returns>
        public static string GetStaticReportsFolder(IConfiguration configuration)
        {
            return DoGetStaticReportsFolder(configuration);
        }

        // INTERNAL
        private static string DoGetStaticReportsFolder(IConfiguration configuration)
        {
            // setup
            var onFolder = configuration.GetValue("rhino:reportConfiguration:reportOut", ".");

            // is current location
            if(onFolder == ".")
            {
                onFolder = Path.Join(Environment.CurrentDirectory, "outputs", "reports", "rhino");
            }
            onFolder = onFolder.Replace(Path.GetFileName(onFolder), string.Empty);

            // setup
            return Path.IsPathRooted(onFolder) ? onFolder : Path.Join(Environment.CurrentDirectory, onFolder);
        }

        /// <summary>
        /// gets a collection of all assemblies where the executing assembly is currently located
        /// </summary>
        /// <returns>assemblies collection</returns>
        public static IEnumerable<Type> GetTypes()
        {
            // get all referenced assemblies
            var referenced = GetReferencedAssemblies();
            var attached = GetAttachedAssemblies(referenced);

            // build assemblies list
            var assemblies = new List<Assembly>();
            assemblies.AddRange(referenced);
            assemblies.AddRange(attached);

            // load all sub-references
            var subReferences = new List<Assembly>();
            foreach (var a in assemblies)
            {
                try
                {
                    var r = a.GetReferencedAssemblies();
                    subReferences.AddRange(r.Select(Assembly.Load));
                    if (a.FullName.Contains("SimpleHtml"))
                    {
                        var c = "break here";
                    }
                }
                catch (Exception e)
                {
                    var b = "";
                    // ignore failed assemblies
                }
            }
            assemblies.AddRange(subReferences);

            // load all assemblies excluding the executing assembly
            var types = new List<Type>();
            foreach (var a in assemblies)
            {
                try
                {
                    types.AddRange(a.GetTypes());
                }
                catch (Exception e) when (e != null)
                {
                    // ignore exceptions
                }
            }
            return types.DistinctBy(i => i.FullName);
        }

        private static IEnumerable<Assembly> GetReferencedAssemblies()
        {
            // shortcuts
            var executing = Assembly.GetExecutingAssembly();
            var calling = Assembly.GetCallingAssembly();
            var entry = Assembly.GetEntryAssembly();

            // initialize results collection
            var assemblies = new List<Assembly> { executing, calling, entry }.Where(r => r != null).ToList();
            var referenced = new List<AssemblyName>
            {
                executing.GetName(),
                calling.GetName(),
                entry?.GetName()
            }
            .Where(r => r != null).ToList();

            // cache all assemblies references
            referenced.TryAddRange(executing?.GetReferencedAssemblies());
            referenced.TryAddRange(calling?.GetReferencedAssemblies());
            referenced.TryAddRange(entry?.GetReferencedAssemblies());

            // load assemblies            
            assemblies.AddRange(referenced.Select(Assembly.Load));
            return assemblies;
        }

        [SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used", Justification = "A special case when need to load by file path.")]
        private static IEnumerable<Assembly> GetAttachedAssemblies(IEnumerable<Assembly> referenced)
        {
            // short-cuts
            var working = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var current = Environment.CurrentDirectory;

            // get all referenced names
            var referencedNames = referenced.Select(r => Path.Combine(working, $"{r.GetName().Name}.dll"));

            // load assemblies
            var w = Directory.GetFiles(working).Where(f => f.EndsWith(".dll") && !referencedNames.Contains(f));
            var c = Directory.GetFiles(current).Where(f => f.EndsWith(".dll") && !referencedNames.Contains(f));

            // build names
            var attachedAssembliesNames = new List<string>();
            attachedAssembliesNames.TryAddRange(w);
            attachedAssembliesNames.TryAddRange(c);

            // build results
            var loadedAssemblies = new List<Assembly>();
            foreach (var a in attachedAssembliesNames)
            {
                try
                {
                    var name = AssemblyName.GetAssemblyName(a);
                    var range = Assembly.Load(name);
                    loadedAssemblies.Add(range);
                }
                catch (Exception e) when (e is FileNotFoundException && !e.Message.Contains("cannot access the file"))
                {
                    try
                    {
                        var range = Assembly.LoadFile(path: a);
                        loadedAssemblies.Add(range);
                    }
                    catch
                    {
                        // ignore exceptions
                    }
                }
                catch (Exception e) when (e != null)
                {
                    // ignore exceptions
                }
            }
            return loadedAssemblies;
        }
    }
}