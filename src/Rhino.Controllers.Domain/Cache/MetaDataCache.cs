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
using System.Text.RegularExpressions;

namespace Rhino.Controllers.Domain.Cache
{
    /// <summary>
    /// Contract for caching meta data.
    /// </summary>
    [DataContract]
    public static partial class MetaDataCache
    {
        #region *** Patterns     ***
        [GeneratedRegex("(?<=\\[test-id]\\s+)\\w+")]
        private static partial Regex GetIdPattern();

        [GeneratedRegex(@"((\r)+)?(\n)+((\r)+)?")]
        private static partial Regex GetNewLinePattern();
        #endregion

        // members: constants
        private const StringComparison Comparison = StringComparison.OrdinalIgnoreCase;
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        // members: cache state
        private static readonly AppSettings s_appSettings = new();
        private static ConcurrentDictionary<string, PluginsCacheModel> s_plugins = GetPluginsCache(Rhino.Api.Extensions.Utilities.Types);

        #region *** Singleton(s) ***
        [DataMember]
        public static IDictionary<string, PluginsCacheModel> Plugins
        {
            get
            {
                s_plugins ??= GetPluginsCache(Rhino.Api.Extensions.Utilities.Types);
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

        // TODO: allow private plugins to be synced
        public static void SyncPlugins(IEnumerable<PluginCacheSyncModel> models)
        {
            foreach (var syncModel in models)
            {
                SyncPlugins(syncModel?.Specification, syncModel?.Authentication, string.Empty);
            }
        }

        public static void SyncPlugins(IEnumerable<PluginCacheSyncModel> models, string dataEncryptionKey)
        {
            foreach (var syncModel in models)
            {
                SyncPlugins(syncModel?.Specification, syncModel?.Authentication, dataEncryptionKey);
            }
        }

        private static void SyncPlugins(string specification, Authentication authentication, string dataEncryptionKey)
        {
            // local
            static IEnumerable<string> GetDirectories(string rootDirectory)
            {
                return Directory
                    .GetDirectories(rootDirectory)
                    .Where(i => Path.GetFileName(i).StartsWith("Rhino", Comparison))
                    .ToArray();
            }

            // setup
            var factory = new RhinoPluginFactory();
            var encryptionKey = dataEncryptionKey;
            var rootDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins"/*Build dynamically from configuration*/);
            var directories = !Directory.Exists(rootDirectory)
                ? Array.Empty<string>()
                : GetDirectories(rootDirectory);

            // clean
            specification = CleanSpecifications(specification);

            // get plugins: rhino
            foreach (var directory in directories)
            {
                SyncPlugins(factory, specification, directory);
            }
        }

        // TODO: always false
        private static void SyncPlugins(RhinoPluginFactory factory, string specification, string directory)
        {
            var id = GetIdPattern().Match(input: specification).Value.Trim();
            var pluginSource = Path.GetFileName(directory);
            var isSource = s_plugins.TryGetValue(pluginSource, out PluginsCacheModel sourceOut);
            var isSpecifications = isSource && s_plugins[pluginSource]?.PluginsCache.TryGetValue(id, out PluginCacheModel pluginOut) == true;
            var cachedPlugin = isSpecifications
                ? CleanSpecifications(s_plugins[pluginSource].PluginsCache[id].Specifications)
                : string.Empty;
            var isMatch = specification.Equals(cachedPlugin);

            if (isMatch) // do not update if there is no change
            {
                return;
            }

            // new cache model
            var plugin = (Source: pluginSource, Plugin: factory.GetRhinoPlugins(specification).FirstOrDefault());
            var pluginCollection = new[] { plugin }.Where(i => i.Plugin != null);
            var pluginCache = GetPluginsCache(pluginCollection);

            // first time cache
            if (!isSource || !isSpecifications)
            {
                s_plugins[pluginSource] = new PluginsCacheModel
                {
                    ActionsCache = new ConcurrentDictionary<string, ActionModel>(Comparer),
                    ActionsCacheByConfiguration = new ConcurrentDictionary<string, ActionModel>(Comparer),
                    PluginsCache = new ConcurrentDictionary<string, PluginCacheModel>(Comparer)
                };
                s_plugins[pluginSource].ActionsCache = pluginCache[pluginSource].ActionsCache;
                s_plugins[pluginSource].PluginsCache = pluginCache[pluginSource].PluginsCache;
                return;
            }

            // update
            _ = s_plugins.TryGetValue(pluginSource, out PluginsCacheModel valueOut);

            valueOut.ActionsCache.TryUpdate(id, pluginCache[pluginSource].ActionsCache[id]);
            valueOut.PluginsCache.TryUpdate(id, pluginCache[pluginSource].PluginsCache[id]);
        }

        #region *** Plugins: Get ***
        private static IEnumerable<(string Source, RhinoPlugin Plugin)> GetPlugins(string directory)
        {
            // setup
            var factory = new RhinoPluginFactory();
            var repository = GetPluginsRepository(directory);
            var pluginsRepository = repository.Select(i => i.Value).ToArray();

            // collect
            var plugins = new List<(string Source, RhinoPlugin Plugin)>();
            foreach (var item in factory.GetRhinoPlugins(pluginsRepository))
            {
                plugins.Add((directory, item));
            }

            // get
            return plugins;
        }

        // TODO: load assemblies domain from folder for Gravity plugins
        private static ConcurrentDictionary<string, PluginsCacheModel> GetPluginsCache(IEnumerable<Type> types)
        {
            // setup
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
                var range = GetPlugins(directory);
                plugins.AddRange(range);
            }

            // new cache
            var cache = GetPluginsCache(plugins);

            // add gravity plugins
            cache["Gravity"] = new()
            {
                ActionsCache = gravityPlugins.GetActionsCache(),
                ActionsCacheByConfiguration = new ConcurrentDictionary<string, ActionModel>(StringComparer.OrdinalIgnoreCase),
                PluginsCache = gravityPlugins.GetPluginsCache()
            };

            // get
            return cache;
        }

        private static ConcurrentDictionary<string, PluginsCacheModel> GetPluginsCache(IEnumerable<(string Source, RhinoPlugin Plugin)> plugins)
        {
            // setup
            var cache = new ConcurrentDictionary<string, PluginsCacheModel>(StringComparer.OrdinalIgnoreCase);

            // collect
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

            // get
            return cache;
        }

        // TODO: encrypt/decrypt plugins body
        private static IDictionary<string, string> GetPluginsRepository(string inDirectory)
        {
            // setup
            var encryptionKey = s_appSettings.StateManager?.DataEncryptionKey ?? string.Empty;

            // setup conditions
            var exists = Directory.Exists(inDirectory);

            // NotFound conditions
            if (!exists)
            {
                return new ConcurrentDictionary<string, string>(Comparer);
            }

            // collect plugins
            var repository = new ConcurrentDictionary<string, string>(Comparer);
            foreach (var file in Directory.GetDirectories(inDirectory).SelectMany(Directory.GetFiles))
            {
                var content = File.ReadAllText(file);
                var id = GetIdPattern().Match(input: content).Value.Trim();
                repository.TryAdd(id, content);
            }

            // get
            return repository;
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
                Specifications = (plugin?.TestSpecifications)
            };
        }

        private static string CleanSpecifications(string specifications)
        {
            // setup
            var lines = GetNewLinePattern()
                .Split(specifications)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrEmpty(i));

            // get
            return string.Join("\n", lines);
        }
        #endregion
    }
}
