/*
 * CHANGE LOG - keep only last 5 threads
 */
using Loader.Contracts;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Loader
{
    /// <summary>
    /// A component for loading all assemblies into the application domain.
    /// </summary>
    public partial class AssembliesLoader
    {
        #region *** Patterns  ***
        [GeneratedRegex("(?i)\\.dll$", RegexOptions.None, "en-US")]
        private static partial Regex GetAssemblyFilePattern();
        #endregion

        #region *** Events    ***
        /// <summary>
        /// Raised right after all <see cref="Assembly"/> have been loaded.
        /// </summary>
        public event EventHandler<AssembliesLoadedEventArgs> AssembliesLoaded;

        /// <summary>
        /// Raised right after a single <see cref="Assembly"/> load.
        /// </summary>
        public event EventHandler<AssembliesLoaderLoadEventArgs> AssemblyLoaded;

        /// <summary>
        /// Raised when assembly load routine encounters an error.
        /// </summary>
        public event EventHandler<AssembliesLoaderErrorEventArgs> AssemblyLoadError;

        /// <summary>
        /// Raised right after the assemblies loading setup routine.
        /// </summary>
        public event EventHandler<AssembliesLoaderSetupEventArgs> SetupCompleted;
        #endregion

        // members: state
        private readonly ConcurrentBag<Assembly> _assemblies;

        /// <summary>
        /// Creates a new instance of AssembliesLoader.
        /// </summary>
        public AssembliesLoader()
        {
            _assemblies = new ConcurrentBag<Assembly>();
        }

        /// <summary>
        /// Gets a collection of <see cref="Assembly"/> right after loaded into the application domain.
        /// </summary>
        /// <returns>A collection of <see cref="Assembly"/></returns>
        public IEnumerable<Assembly> GetAssemblies()
        {
            return ExtractTypes(string.Empty, Array.Empty<string>()).Select(i => i.Assembly);
        }

        /// <summary>
        /// Gets a collection of <see cref="Assembly"/> right after loaded into the application domain.
        /// </summary>
        /// <param name="directory">The folder under which to find and load the assemblies.</param>
        /// <returns>A collection of <see cref="Assembly"/></returns>
        public IEnumerable<Assembly> GetAssemblies(string directory)
        {
            return ExtractTypes(directory, Array.Empty<string>()).Select(i => i.Assembly);
        }

        /// <summary>
        /// Gets a collection of <see cref="Assembly"/> right after loaded into the application domain.
        /// </summary>
        /// <param name="directory">The folder under which to find and load the assemblies.</param>
        /// <param name="directories">Additional folders under which to find and load the assemblies (relative to rootFolder).</param>
        /// <returns>A collection of <see cref="Assembly"/></returns>
        public IEnumerable<Assembly> GetAssemblies(string directory, IEnumerable<string> directories)
        {
            return ExtractTypes(directory, directories).Select(i => i.Assembly);
        }

        /// <summary>
        /// Gets a collection of <see cref="Type"/> right after loaded into the application domain.
        /// </summary>
        /// <returns>A collection of <see cref="Type"/></returns>
        public IEnumerable<Type> GetTypes()
        {
            return ExtractTypes(string.Empty, Array.Empty<string>()).SelectMany(i => i.Types);
        }

        /// <summary>
        /// Gets a collection of <see cref="Type"/> right after loaded into the application domain.
        /// </summary>
        /// <param name="directory">The folder under which to find and load the types.</param>
        /// <returns>A collection of <see cref="Type"/></returns>
        public IEnumerable<Type> GetTypes(string directory)
        {
            return ExtractTypes(directory, Array.Empty<string>()).SelectMany(i => i.Types);
        }

        /// <summary>
        /// Gets a collection of <see cref="Type"/> right after loaded into the application domain.
        /// </summary>
        /// <param name="directory">The folder under which to find and load the types.</param>
        /// <param name="directories">Additional folders under which to find and load the types (relative to rootFolder).</param>
        /// <returns>A collection of <see cref="Type"/></returns>
        public IEnumerable<Type> GetTypes(string directory, IEnumerable<string> directories)
        {
            return ExtractTypes(directory, directories).SelectMany(i => i.Types);
        }

        /// <summary>
        /// Gets a collection of <see cref="Assembly"/> and the collection of <see cref="Type"/> that belongs to it.
        /// </summary>
        /// <returns>A collection of <see cref="Assembly"/> and the collection of <see cref="Type"/>.</returns>
        public IEnumerable<(Assembly Assembly, IEnumerable<Type> Types)> GetMap()
        {
            return ExtractTypes(string.Empty, Array.Empty<string>());
        }

        /// <summary>
        /// Gets a collection of <see cref="Assembly"/> and the collection of <see cref="Type"/> that belongs to it.
        /// </summary>
        /// <param name="directory">The folder under which to find and load the types.</param>
        /// <returns>A collection of <see cref="Assembly"/> and the collection of <see cref="Type"/>.</returns>
        public IEnumerable<(Assembly Assembly, IEnumerable<Type> Types)> GetMap(string directory)
        {
            return ExtractTypes(directory, Array.Empty<string>());
        }

        /// <summary>
        /// Gets a collection of <see cref="Assembly"/> and the collection of <see cref="Type"/> that belongs to it.
        /// </summary>
        /// <param name="directory">The folder under which to find and load the types.</param>
        /// <returns>A collection of <see cref="Assembly"/> and the collection of <see cref="Type"/>.</returns>
        public IEnumerable<(Assembly Assembly, IEnumerable<Type> Types)> GetMap(string directory, IEnumerable<string> directories)
        {
            return ExtractTypes(directory, directories);
        }

        #region *** Utilities ***
        private IEnumerable<(Assembly Assembly, IEnumerable<Type> Types)> ExtractTypes(string directory, IEnumerable<string> directories)
        {
            // reset
            _assemblies.Clear();

            // build
            directory = string.IsNullOrEmpty(directory) || directory == "."
                ? Environment.CurrentDirectory
                : directory;

            // setup
            var _rootFolder = string.IsNullOrEmpty(directory)
                ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                : directory;
            var rootFolders = new[]
            {
                _rootFolder
            };

            var _pluginsFolders = directories.Select(i => Path.Combine(_rootFolder, i));
            var locations = _pluginsFolders
                .Where(i => Directory.Exists(i))
                .SelectMany(i => Directory.GetDirectories(i))
                .Concat(rootFolders);

            // build files
            var files = GetFiles(locations);

            // event
            SetupCompleted?.Invoke(this, new AssembliesLoaderSetupEventArgs
            {
                Directories = locations,
                Directory = _rootFolder,
                Files = files
            });

            // build
            foreach (var assemblyFile in files)
            {
                InvokeGetAssemblies(assemblyFile);
            }

            // invoke
            var assembliesCollection = _assemblies.Select(i => GetPair(i)).Where(i => i.Assembly != null);

            // event
            AssembliesLoaded?.Invoke(sender: this, e: new AssembliesLoadedEventArgs
            {
                AssembliesCollection = assembliesCollection
            });

            // get
            return assembliesCollection;
        }

        private static IEnumerable<string> GetFiles(IEnumerable<string> locations)
        {
            // local
            static IEnumerable<string> Get(string location)
            {
                // setup
                var filesCollection = new List<string>();

                // not found
                if (!Directory.Exists(location))
                {
                    return filesCollection;
                }

                // get
                var files = Directory.GetFiles(location).Where(i => GetAssemblyFilePattern().IsMatch(input: i));
                var directories = Directory.GetDirectories(location);

                // add
                filesCollection.AddRange(files);

                // recurse
                foreach (var directory in directories)
                {
                    var collection = Get(location: directory);
                    filesCollection.AddRange(collection);
                }

                // get
                return filesCollection;
            }

            // collect
            var files = new List<string>();
            foreach (var location in locations)
            {
                var collection = Get(location);
                files.AddRange(collection);
            }

            // get
            return files;
        }

        private void InvokeGetAssemblies(string assemblyFile)
        {
            // setup
            var assembly = InvokeGetAssembly(assemblyFile);

            // bad request
            if (assembly == null)
            {
                return;
            }

            // build
            _assemblies.Add(assembly);
            foreach (var item in assembly.GetReferencedAssemblies())
            {
                try
                {
                    var names = _assemblies.Select(i => i.FullName).Any(i => i == item.FullName);
                    if (names)
                    {
                        continue;
                    }
                    var referenced = Assembly.Load(item);
                    InvokeGetAssemblies(referenced.Location);
                }
                catch (Exception e) when (e != null)
                {
                    Trace.TraceWarning(e.Message, e);
                }
            }
        }

        [SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used", Justification = "Must be loaded from a file")]
        private Assembly InvokeGetAssembly(string assemblyFile)
        {
            // setup
            Assembly assembly = null;

            // invoke
            try
            {
                assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFile));
                assembly.GetTypes();
            }
            catch (FileNotFoundException)
            {
                if (!File.Exists(assemblyFile))
                {
                    return assembly;
                }
                assembly = Assembly.LoadFile(assemblyFile);
                try
                {
                    assembly.GetTypes();
                }
                catch (Exception e)
                {
                    AssemblyLoadError?.Invoke(this, new AssembliesLoaderErrorEventArgs
                    {
                        Exception = e,
                        File = assemblyFile,
                        Source = nameof(InvokeGetAssemblies)
                    });
                    Trace.TraceWarning(e.Message);
                }
            }
            catch (Exception e) when (e != null)
            {
                AssemblyLoadError?.Invoke(this, new AssembliesLoaderErrorEventArgs
                {
                    Exception = e,
                    File = assemblyFile,
                    Source = nameof(InvokeGetAssemblies)
                });
                Trace.TraceWarning(e.Message);
            }

            // get
            return assembly;
        }

        private (Assembly Assembly, IEnumerable<Type> Types) GetPair(Assembly assembly)
        {
            try
            {
                // setup
                var types = assembly.GetTypes();

                // event
                AssemblyLoaded?.Invoke(this, new AssembliesLoaderLoadEventArgs
                {
                    Assembly = assembly,
                    Types = types
                });

                // get
                return (assembly, types);
            }
            catch (Exception e) when (e != null)
            {
                // event
                AssemblyLoadError?.Invoke(this, new AssembliesLoaderErrorEventArgs
                {
                    Exception = e,
                    File = Path.Combine(assembly.Location, assembly.FullName),
                    Source = nameof(GetPair)
                });
                Trace.TraceWarning(e.Message, e);
            }
            return (null, null);
        }
        #endregion
    }
}
