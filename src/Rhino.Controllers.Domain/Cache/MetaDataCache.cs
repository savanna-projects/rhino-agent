/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Loader;
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

using Microsoft.CodeAnalysis;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Parser;
using Rhino.Controllers.Domain.Extensions;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;
using Rhino.Settings;

using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Domain.Cache
{
    /// <summary>
    /// Contract for caching meta data.
    /// </summary>
    [DataContract]
    public static class MetaDataCache
    {
        // members: cache state
        private static readonly AppSettings s_appSettings = new();
        private static ConcurrentDictionary<string, PluginsCacheModel> s_plugins = GetPluginsCache(Utilities.Types);

        #region *** Singleton(s) ***
        [DataMember]
        public static IDictionary<string, PluginsCacheModel> Plugins
        {
            get
            {
                s_plugins ??= GetPluginsCache(Utilities.Types);
                return s_plugins;
            }
        }
        #endregion

        // Use for hot load
        public static void SyncCache()
        {
            // reload all types
            var types = new AssembliesLoader().GetTypes();

            // refresh cache
            s_plugins = GetPluginsCache(types);
        }

        public static void SyncPlugins(IEnumerable<PluginCacheSyncModel> models)
        {
            foreach (var syncModel in models)
            {
                SyncPlugins(syncModel?.Specification, syncModel?.Authentication);
            }
        }

        private static void SyncPlugins(string specification, Authentication authentication)
        {
            // setup
            var factory = new RhinoPluginFactory();
            var rootDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins"/*Build dynamically from configuration*/);
            var directories = !Directory.Exists(rootDirectory)
                ? Array.Empty<string>()
                : Directory
                    .GetDirectories(rootDirectory)
                    .Where(i => Path.GetFileName(i)
                    .StartsWith("Rhino", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

            // get plugins: rhino
            var plugins = new ConcurrentBag<(string Source, RhinoPlugin Plugin)>();
            foreach (var directory in directories)
            {
                foreach (var item in GetPluginsRepository(directory))
                {
                    var isEqual = "";
                }
            }
            // TODO: read all plugin files
            // TODO: compare with plugins input
            // TODO: update collection when input does not match
            //// setup
            //var path = Path.Combine(models.First().Path, RhinoPluginEntry.PluginsRhinoSpecFile);

            //// not found
            //if (!File.Exists(path))
            //{
            //    return;
            //}

            //// read
            //var pluginContent = File.ReadAllText(models.First().Path);
            //var isEqual = pluginContent.DeepEqual(models.First().Specifications);
            //if (isEqual)
            //{
            //    return;
            //}

            //// create
            //var factory = new RhinoPluginFactory();
            //var p = factory.GetRhinoPlugins(pluginContent);
        }

        #region *** Plugins: Get ***
        // TODO: load assemblies domain from folder for Gravity plugins
        private static ConcurrentDictionary<string, PluginsCacheModel> GetPluginsCache(IEnumerable<Type> types)
        {
            // setup
            var cache = new ConcurrentDictionary<string, PluginsCacheModel>(StringComparer.OrdinalIgnoreCase);
            var factory = new RhinoPluginFactory();
            var rootDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins"/*Build dynamically from configuration*/);
            var directories = !Directory.Exists(rootDirectory)
                ? Array.Empty<string>()
                : Directory
                    .GetDirectories(rootDirectory)
                    .Where(i => Path.GetFileName(i)
                    .StartsWith("Rhino", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

            // get plugins: code
            var gravityPlugins = types.GetActionAttributes();

            // get plugins: rhino
            var plugins = new List<(string Source, RhinoPlugin Plugin)>();
            foreach (var directory in directories)
            {
                var repository = GetPluginsRepository(directory).ToArray();
                foreach (var item in factory.GetRhinoPlugins(repository))
                {
                    plugins.Add((directory, item));
                }
            }

            // build cache
            foreach (var group in plugins.GroupBy(i => i.Source))
            {
                var groupCollection = new ConcurrentDictionary<string, PluginCacheModel>(StringComparer.OrdinalIgnoreCase);
                var key = Path.GetFileName(group.Key);
                foreach (var item in group.Select(i => (Path: i.Source, i.Plugin, Attribute: i.Plugin.ToAttribute())))
                {
                    var path = Path.Combine(item.Path, item.Plugin.Key);
                    groupCollection[item.Plugin.Key] = GetPluginCacheModel(
                        source: ActionModel.ActionSource.Plugin,
                        path,
                        item.Plugin,
                        item.Attribute);
                }
                cache[key] = new()
                {
                    ActionsCache = groupCollection.GetActionsCache(),
                    ActionsCacheByConfiguration = new ConcurrentDictionary<string, ActionModel>(StringComparer.OrdinalIgnoreCase),
                    PluginsCache = groupCollection
                };
            }
            cache["Gravity"] = new()
            {
                ActionsCache = gravityPlugins.GetActionsCache(),
                ActionsCacheByConfiguration = new ConcurrentDictionary<string, ActionModel>(StringComparer.OrdinalIgnoreCase),
                PluginsCache = gravityPlugins.GetPluginsCache()
            };

            // get
            return cache;
        }

        // TODO: encrypt/decrypt plugins body
        private static IEnumerable<string> GetPluginsRepository(string inDirectory)
        {
            // setup
            var encryptionKey = s_appSettings.StateManager?.DataEncryptionKey ?? string.Empty;

            // setup conditions
            var exists = Directory.Exists(inDirectory);

            // NotFound conditions
            if (!exists)
            {
                return Array.Empty<string>();
            }

            // collect plugins
            return Directory
                .GetDirectories(inDirectory)
                .SelectMany(Directory.GetFiles)
                .Select(File.ReadAllText);
        }

        // TODO: this is a duplicate method with Rhino.Controllers.Domain.Extensions.GetPluginCacheModel
        private static PluginCacheModel GetPluginCacheModel(string source, string path, RhinoPlugin plugin, ActionAttribute attribute)
        {
            // setup
            var actionModel = new ActionModel
            {
                Entity = attribute,
                Key = attribute.Name,
                Literal = attribute.Name.ToSpaceCase().ToLower(),
                Source = source,
                Verb = "TBD"
            };

            // get
            return new()
            {
                ActionModel = actionModel,
                Directory = Path.GetFileName(path),
                Path = Path.Exists(path) ? path : null,
                Plugin = plugin,
                Specifications = plugin == default ? null : plugin?.ToString()
            };
        }
        #endregion
    }
}
