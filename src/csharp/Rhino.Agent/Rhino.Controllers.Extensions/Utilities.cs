/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for <see cref="Stream"/> object and other related object.
    /// </summary>
    public static class Utilities
    {
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
                }
                catch
                {
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
