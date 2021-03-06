﻿/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using Rhino.Api.Contracts.AutomationProvider;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
        private static readonly ILogger logger = new TraceLogger("RhinoApi", nameof(Utilities));
        private static readonly IList<Assembly> assemblies = new List<Assembly>();

        /// <summary>
        /// Gets a distinct collection of <see cref="Type"/> loaded into the AppDomain.
        /// </summary>
        public static IEnumerable<Type> Types => GetTypes().SelectMany(i => i.Types).Distinct();

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
            assemblies.Clear();

            // setup
            var mainLocation = string.IsNullOrEmpty(root)
                ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                : root;
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
            return assemblies.Select(i => GetPair(i)).Where(i => i.Assembly != null);
        }

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
                    logger?.Warn(e.Message);
                    return;
                }
            }
            catch (Exception e) when (e != null)
            {
                logger?.Warn(e.Message);
                return;
            }

            // build
            assemblies.Add(assembly);
            foreach (var item in assembly.GetReferencedAssemblies())
            {
                try
                {
                    var names = assemblies.Select(i => i.FullName).Any(i => i == item.FullName);
                    if (names)
                    {
                        continue;
                    }
                    var referenced = Assembly.Load(item);
                    GetAssemblies(referenced.Location);
                }
                catch (Exception e) when (e != null)
                {
                    logger?.Warn(e.Message, e);
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
                logger?.Warn(e.Message, e);
            }
            return (null, null);
        }
        #endregion

        #region *** Graphics   ***
        /// <summary>
        /// Renders RhinoAPI logo in the console.
        /// </summary>
        public static void RenderLogo()
        {
            try
            {
                DoRenderLogo(1, 1, ConsoleColor.Black, ConsoleColor.White, Rhino());
                DoRenderLogo(1, 55, ConsoleColor.Black, ConsoleColor.Red, Api());

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine();
            }
            catch (Exception e) when (e != null)
            {
                // ignore errors
            }
        }

        private static void DoRenderLogo(
            int startRow,
            int startColumn,
            ConsoleColor background,
            ConsoleColor foreground,
            IEnumerable<string> lines)
        {
            // setup
            Console.CursorTop = startRow;
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;

            // render
            for (int i = 0; i < lines.Count(); i++)
            {
                Console.CursorTop = startRow + i;
                Console.CursorLeft = startColumn;
                Console.WriteLine(lines.ElementAt(i));
            }
        }

        private static IEnumerable<string> Rhino() => new List<string>
        {
            "88888888ba   88           88                          ",
            "88      \"8b  88           \"\"                          ",
            "88      ,8P  88                                       ",
            "88aaaaaa8P'  88,dPPYba,   88  8b,dPPYba,    ,adPPYba, ",
            "88\"\"\"\"88'    88P'    \"8a  88  88P'   `\"8a  a8\"     \"8a",
            "88    `8b    88       88  88  88       88  8b       d8",
            "88     `8b   88       88  88  88       88  \"8a,   ,a8\"",
            "88      `8b  88       88  88  88       88   `\"YbbdP\"' ",
        };

        private static IEnumerable<string> Api() => new List<string>
        {
            "        db         88888888ba   88",
            "       d88b        88      \"8b  88",
            "      d8'`8b       88      ,8P  88",
            "     d8'  `8b      88aaaaaa8P'  88",
            "    d8YaaaaY8b     88\"\"\"\"\"\"'    88",
            "   d8\"\"\"\"\"\"\"\"8b    88           88",
            "  d8'        `8b   88           88",
            " d8'          `8b  88           88",
        };
        #endregion
    }
}