/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;
using Gravity.Services.DataContracts;

using Newtonsoft.Json;

using Rhino.Api.Contracts.AutomationProvider;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Rhino.Agent.Domain
{
    public class RhinoPluginRepository : Repository
    {
        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Domain.RhinoPluginRepository.</param>
        public RhinoPluginRepository(IServiceProvider provider) : base(provider)
        { }

        #region *** GET    ***
        /// <summary>
        /// Gets all Plugins under context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get non public plugins.</param>
        /// <returns>A collection of RhinoPlugin specs.</returns>
        public (HttpStatusCode statusCode, IEnumerable<string> data) Get(Authentication authentication)
        {
            // get
            var plugins = DoGet(authentication);

            // setup: status
            var statusCode = !plugins.Any() ? HttpStatusCode.NotFound : HttpStatusCode.OK;

            // get configuration
            return (statusCode, plugins);
        }

        /// <summary>
        /// Gets a single RhinoPlugin from context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get RhinoPlugin.</param>
        /// <param name="id">Rhino Plugin unique name by which to find this RhinoPlugin.</param>
        /// <returns>A RhinoPlugin spec.</returns>
        public (HttpStatusCode statusCode, IEnumerable<string> data) Get(Authentication authentication, string id)
        {
            // get
            var plugin = DoGet(authentication).FirstOrDefault(i => Regex.IsMatch(i, @"(?i)(?<=\[test-id]\s+)" + id));

            // setup: status
            var statusCode = plugin == default ? HttpStatusCode.NotFound : HttpStatusCode.OK;

            // get configuration
            return (statusCode, new[] { plugin });
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Repository methods cannot be static")]
        private IEnumerable<string> DoGet(Authentication authentication)
        {
            // setup
            var path = $"{Environment.CurrentDirectory}/{RhinoPluginEntry.PluginsRhinoFolder}";
            var userPath = string.IsNullOrEmpty(authentication.UserName) || string.IsNullOrEmpty(authentication.Password)
                ? path
                : path + "-" + JsonConvert.SerializeObject(authentication).ToBase64();

            // setup conditions
            var isPublic = Directory.Exists(path);
            var isPrivate = Directory.Exists(userPath);

            // NotFound conditions
            if (!isPublic && !isPrivate)
            {
                return Array.Empty<string>();
            }

            // collect plugins
            var onPlugins = new List<string>();
            if (isPublic)
            {
                var collection = Directory.GetDirectories(path).SelectMany(Directory.GetFiles).Select(File.ReadAllText);
                onPlugins.AddRange(collection);
            }
            if (isPrivate && !path.Equals(userPath, StringComparison.OrdinalIgnoreCase))
            {
                var collection = Directory.GetDirectories(userPath).SelectMany(Directory.GetFiles).Select(File.ReadAllText);
                onPlugins.AddRange(collection);
            }

            // results
            return onPlugins;
        }
        #endregion

        #region *** POST   ***
        /// <summary>
        /// Gets all Plugins under context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get non public plugins.</param>
        /// <param name="pluginSpecs">RhinoPlugin specs by which to create plugins.</param>
        /// <param name="isPrivate">Set to <see cref="true"/> to create this plugin as a private plugin.</param>
        /// <returns>A collection of RhinoPlugin specs.</returns>
        public (HttpStatusCode statusCode, IEnumerable<string> data) Post(Authentication authentication, IEnumerable<string> pluginSpecs, bool isPrivate)
        {
            // setup
            var basePath = $"{Environment.CurrentDirectory}/{RhinoPluginEntry.PluginsRhinoFolder}";
            var path = isPrivate
                ? basePath + "-" + JsonConvert.SerializeObject(authentication).ToBase64()
                : basePath;

            // iterate
            var exceptions = new List<Exception>();
            foreach (var spec in pluginSpecs)
            {
                var exception = WritePluginSpec(path, spec);
                if (exception != default)
                {
                    exceptions.Add(exception);
                }
            }

            // response
            return exceptions.Count > 0
                ? (HttpStatusCode.OK, exceptions.Select(i => i.Message))
                : (HttpStatusCode.Created, Array.Empty<string>());
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Repository methods cannot be static")]
        private Exception WritePluginSpec(string path, string spec)
        {
            Exception exception = default;
            try
            {
                var id = Regex.Match(input: spec, pattern: @"(?i)(?<=\[test-id]\s+)\w+").Value;
                var pluginPath = Path.Combine(path, id);
                var pluginFilePath = Path.Combine(pluginPath, RhinoPluginEntry.PluginsRhinoSpecFile);

                Directory.CreateDirectory(pluginPath);
                File.WriteAllText(path: pluginFilePath, spec);
            }
            catch (Exception e) when (e != null)
            {
                return e;
            }
            return exception;
        }
        #endregion

        #region *** DELETE ***
        /// <summary>
        /// Delete all Plugins under context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get non public plugins.</param>
        /// <returns>Status code and <see cref="Exception"/> if thrown.</returns>
        public (HttpStatusCode statusCode, Exception data) Delete(Authentication authentication)
        {
            try
            {
                // setup
                var path = $"{Environment.CurrentDirectory}/{RhinoPluginEntry.PluginsRhinoFolder}";
                var userPath = string.IsNullOrEmpty(authentication.UserName) || string.IsNullOrEmpty(authentication.Password)
                    ? path
                    : path + "-" + JsonConvert.SerializeObject(authentication).ToBase64();

                // delete public
                DeleteFolder(userPath);
            }
            catch (Exception e) when (e != null)
            {
                return (HttpStatusCode.InternalServerError, e);
            }
            return (HttpStatusCode.NoContent, default);
        }

        /// <summary>
        /// Delete RhinoPlugin under context.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get non public plugins.</param>
        /// <returns>Status code and <see cref="Exception"/> if thrown.</returns>
        public (HttpStatusCode statusCode, Exception data) Delete(Authentication authentication, string id)
        {
            try
            {
                // setup
                var path = $"{Environment.CurrentDirectory}/{RhinoPluginEntry.PluginsRhinoFolder}";
                var userPath = string.IsNullOrEmpty(authentication.UserName) || string.IsNullOrEmpty(authentication.Password)
                    ? path
                    : path + "-" + JsonConvert.SerializeObject(authentication).ToBase64();

                // setup conditions
                var isPublic = Directory.Exists(Path.Combine(path, id));
                var isPrivate = Directory.Exists(Path.Combine(userPath, id));

                // exit conditions
                if (!isPublic && !isPrivate)
                {
                    return (HttpStatusCode.NotFound, default);
                }

                // delete from public
                if (isPublic)
                {
                    DeleteFolder(path: Path.Combine(path, id));
                }

                // delete from private
                if (isPrivate)
                {
                    DeleteFolder(path: Path.Combine(userPath, id));
                }
            }
            catch (Exception e) when (e != null)
            {
                return (HttpStatusCode.InternalServerError, e);
            }
            return (HttpStatusCode.NoContent, default);
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Repository methods cannot be static")]
        private void DeleteFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }
            Directory.Delete(path, recursive: true);
        }
        #endregion
    }
}
