/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using OpenQA.Selenium.Remote;

using Rhino.Api.Contracts.AutomationProvider;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Rhino.Agent.Domain
{
    public class RhinoPluginRepository : Repository
    {
        // members: state
        private readonly RhinoConfigurationRepository configurationRepository;

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Domain.RhinoPluginRepository.</param>
        public RhinoPluginRepository(IServiceProvider provider) : base(provider)
        {
            configurationRepository = provider.GetRequiredService<RhinoConfigurationRepository>();
        }

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

        private IEnumerable<string> DoGet(Authentication authentication)
        {
            // setup
            var path = $"{Environment.CurrentDirectory}/{RhinoPluginEntry.PluginsRhinoFolder}";
            var userPath = string.IsNullOrEmpty(authentication.UserName) || string.IsNullOrEmpty(authentication.Password)
                ? string.Empty
                : path + "-" + JsonConvert.SerializeObject(authentication).ToBase64();

            // NotFound conditions
            if (!Directory.Exists(path) || Directory.GetDirectories(path).Length == 0)
            {
                return Array.Empty<string>();
            }

            // collect
            var plugins = Directory.GetDirectories(path).SelectMany(Directory.GetFiles).Select(File.ReadAllText);
            var userPlugins = string.IsNullOrEmpty(userPath)
                ? Array.Empty<string>()
                : Directory.GetDirectories(userPath).SelectMany(Directory.GetFiles).Select(File.ReadAllText);

            // results
            return plugins.Concat(userPlugins);
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

                // exit conditions
                if (!Directory.Exists(userPath))
                {
                    return (HttpStatusCode.NotFound, default);
                }

                // get plugin for delete
                var forDelete = Array.Find(Directory
                    .GetDirectories(userPath), i => new DirectoryInfo(i).Name.Equals(id, StringComparison.OrdinalIgnoreCase));

                // exit conditions
                if (!Directory.Exists(forDelete))
                {
                    return (HttpStatusCode.NotFound, default);
                }

                // delete
                DeleteFolder(forDelete);
            }
            catch (Exception e) when (e != null)
            {
                return (HttpStatusCode.InternalServerError, e);
            }
            return (HttpStatusCode.NoContent, default);
        }

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
