/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Extensions;

using LiteDB;

using Microsoft.AspNetCore.Http;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models.Server;
using Rhino.Settings;

using System.IO.Compression;
using System.Text.RegularExpressions;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Rhino.Controllers.Domain.Automation
{
    /// <summary>
    /// Data Access Layer for Rhino API plugins repository.
    /// </summary>
    public partial class PluginsRepository : Repository<string>, IPluginsRepository
    {
        // patterns
        [GeneratedRegex("(?i)(?<=\\[test-id]\\s+)\\w+", RegexOptions.None, "en-US")]
        private static partial Regex GetTestIdPattern();

        // members: static
        private static readonly string s_basePath = Path.Combine(Environment.CurrentDirectory, "Plugins");
        private static readonly IDictionary<string, string> s_folders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Gravity"] = Path.Combine(s_basePath, "Gravity"),
            ["Reporter"] = Path.Combine(s_basePath, "Reporters"),
            ["Connector"] = Path.Combine(s_basePath, "Connectors")
        };

        // members: state
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        /// <param name="liteDb">An ILiteDatabase implementation to use with the Repository.</param>
        /// <param name="appSettings">An IConfiguration implementation to use with the Repository.</param>
        public PluginsRepository(ILogger logger, ILiteDatabase liteDb, AppSettings appSettings)
            : base(logger, liteDb, appSettings)
        {
            _logger = logger.CreateChildLogger(nameof(EnvironmentRepository));
        }

        #region *** Add    ***
        /// <summary>
        /// Add a new plugin to the domain state.
        /// </summary>
        /// <param name="entity">The PluginSpec object to post.</param>
        /// <returns>The id of the RhinoPlugin.</returns>
        public override string Add(string entity)
        {
            return Add(false, new[] { entity });
        }

        /// <summary>
        /// Add a new plugin to the domain state.
        /// </summary>
        /// <param name="entity">The PluginSpec object to post.</param>
        /// <param name="isPrivate">A value indicates if the plugin will be created as private or public.</param>
        /// <returns>The id of the RhinoPlugin.</returns>
        /// <remarks>Private plugins can only be modified by the use created them.</remarks>
        public string Add(IEnumerable<string> entity, bool isPrivate)
        {
            return Add(isPrivate, entity);
        }

        private string Add(bool isPrivate, IEnumerable<string> entity)
        {
            // setup
            var basePath = Path.Combine(Environment.CurrentDirectory, RhinoPluginEntry.PluginsRhinoFolder);
            var encryptionKey = AppSettings.StateManager?.DataEncryptionKey ?? string.Empty;
            var path = isPrivate
                ? basePath + "-" + JsonSerializer.Serialize(Authentication).ToBase64().Encrypt(encryptionKey).RemoveNonWord()
                : basePath;

            // iterate
            var exceptions = new List<Exception>();
            foreach (var spec in entity)
            {
                var exception = WritePluginSpec(path, spec);
                if (exception != default)
                {
                    exceptions.Add(exception);
                }
            }

            // response
            var responseBody = exceptions.Count > 0
                ? string.Empty
                : string.Join(Environment.NewLine + ">>>" + Environment.NewLine, entity);

            // get
            if (exceptions.Count == 0)
            {
                _logger?.Debug($"Create-Plugin -Private {isPrivate} = Created");
                return responseBody;
            }

            // error
            foreach (var exception in exceptions)
            {
                _logger?.Error($"Create-Plugin -Private {isPrivate} = InternalServerError", exception);
            }

            // get
            return responseBody;
        }

        private static Exception WritePluginSpec(string path, string spec)
        {
            Exception exception = default;
            try
            {
                var id = GetTestIdPattern().Match(input: spec).Value;
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

        /// <summary>
        /// Submits a code package into Rhino Domain.
        /// </summary>
        /// <param name="uploadModel">Package information.</param>
        /// <returns>Submit results</returns>
        /// <remarks>Please note, you need to restart the server or reload the domain in order to apply the changes.</remarks>
        public async Task<(int StatusCode, string Message)> SubmitAsync(PackageUploadModel uploadModel)
        {
            try
            {
                // bad request
                if (!s_folders.ContainsKey(uploadModel.PackageType))
                {
                    var badReqeust =
                        $"The package type, {uploadModel?.PackageType} is not supported. " +
                        "Please provide a valid package type (e.g., `Gravity`, `Reporter` or `Connector`)";
                    return (StatusCodes.Status400BadRequest, badReqeust);
                }

                // setup
                var pluginsDirecory = Path.Combine(s_folders[uploadModel.PackageType]);
                var packageFile = $"{uploadModel.Id}.zip";
                var packageDirectory = Path.Combine(pluginsDirecory, uploadModel.Id);
                var packagePath = Path.Combine(pluginsDirecory, packageFile);
                var bytes = Convert.FromBase64String(uploadModel.FileData);

                // clean
                if (Directory.Exists(packageDirectory))
                {
                    Directory.Delete(packageDirectory, true);
                }

                // write
                Directory.CreateDirectory(packageDirectory);
                await File.WriteAllBytesAsync(packagePath, bytes);

                // extract
                ZipFile.ExtractToDirectory(packagePath, packageDirectory, true);

                // clean
                File.Delete(packagePath);

                // get
                var created = $"Submit-Plugin -Id {uploadModel.Id} -Type {uploadModel.PackageType} = Created";
                _logger?.Debug(created);
                return (StatusCodes.Status201Created, created);
            }
            catch (Exception e) when (e != null)
            {
                return (StatusCodes.Status500InternalServerError, e.GetBaseException().Message);
            }
        }

        // TODO: add folders from configuration
        public (int StatusCode, string Message) SyncAssemblies() => Controllers
            .Extensions
            .Utilities
            .SyncAssemblies();
        #endregion

        #region *** Delete ***
        /// <summary>
        /// Deletes a plugin from the domain state.
        /// </summary>
        /// <param name="id">The plugin id by which to delete.</param>
        /// <returns><see cref="int"/>.</returns>
        public override int Delete(string id)
        {
            // setup conditions
            var isUser = !string.IsNullOrEmpty(Authentication.Username);
            var isPassword = !string.IsNullOrEmpty(Authentication.Password);

            // setup
            var path = Path.Combine(Environment.CurrentDirectory, RhinoPluginEntry.PluginsRhinoFolder);
            var encryptionKey = AppSettings.StateManager?.DataEncryptionKey ?? string.Empty;
            var privateKey = "-" + JsonSerializer.Serialize(Authentication).ToBase64().Encrypt(encryptionKey).RemoveNonWord();
            var pluginsPath = !isUser && !isPassword ? path : path + privateKey;
            var userPath = string.IsNullOrEmpty(id) ? pluginsPath : Path.Combine(pluginsPath, id);
            var publicPath = Path.Combine(path, id);

            // setup conditions
            var isPrivate = Directory.Exists(userPath);
            var isPublic = Directory.Exists(publicPath);

            // not found
            if (!isPublic && !isPrivate)
            {
                return StatusCodes.Status404NotFound;
            }

            // setup
            var pluginPath = isPublic ? publicPath : userPath;

            // get
            return DeleteFolder(pluginPath) != default
                ? StatusCodes.Status500InternalServerError
                : StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Deletes all plugins from the domain state.
        /// </summary>
        /// <returns><see cref="int"/>.</returns>
        public override int Delete()
        {
            // setup conditions
            var isUser = !string.IsNullOrEmpty(Authentication.Username);
            var isPassword = !string.IsNullOrEmpty(Authentication.Password);

            // setup
            var path = Path.Combine(Environment.CurrentDirectory, RhinoPluginEntry.PluginsRhinoFolder);
            var encryptionKey = AppSettings.StateManager?.DataEncryptionKey ?? string.Empty;
            var privateKey = "-" + JsonSerializer.Serialize(Authentication).ToBase64().Encrypt(encryptionKey).RemoveNonWord();
            var userPath = !isUser && !isPassword ? path : path + privateKey;

            // get
            return DeleteFolder(userPath) != default
                ? StatusCodes.Status500InternalServerError
                : StatusCodes.Status204NoContent;
        }

        private Exception DeleteFolder(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    _logger?.Debug($"Delete-Plugin -Path {path} = (NotFound, NoPlugin)");
                    return default;
                }

                Directory.Delete(path, recursive: true);
                _logger?.Debug($"Delete-Plugin -Path {path} = NoContent");

                return default;
            }
            catch (Exception e) when (e != null)
            {
                _logger?.Debug($"Delete-Plugin -Path {path} = InternalServerError", e);
                return e;
            }
        }
        #endregion

        #region *** Get    ***
        /// <summary>
        /// Gets all plugins in the domain collection.
        /// </summary>
        /// <returns>A Collection of RhinoPlugin.</returns>
        public override IEnumerable<string> Get()
        {
            return InvokeGet();
        }

        /// <summary>
        /// Gets a plugin from the domain state.
        /// </summary>
        /// <param name="id">The environment id by which to get.</param>
        /// <returns><see cref="int"/> and KeyValuePair<string, object> object (if any).</returns>
        public override (int StatusCode, string Entity) Get(string id)
        {
            // get
            var plugin = InvokeGet().FirstOrDefault(i => Regex.IsMatch(i, @"(?i)(?<=\[test-id]\s+)" + id));

            // setup: status
            var statusCode = plugin == default
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status200OK;

            // get configuration
            return (statusCode, plugin);
        }

        private IEnumerable<string> InvokeGet()
        {
            // setup conditions
            var isUser = !string.IsNullOrEmpty(Authentication.Username);
            var isPassword = !string.IsNullOrEmpty(Authentication.Password);

            // setup
            var path = Path.Combine(Environment.CurrentDirectory, RhinoPluginEntry.PluginsRhinoFolder);
            var encryptionKey = AppSettings.StateManager?.DataEncryptionKey ?? string.Empty;
            var privateKey = "-" + JsonSerializer
                .Serialize(Authentication)
                .ToBase64()
                .Encrypt(encryptionKey)
                .RemoveNonWord();
            var userPath = !isUser && !isPassword ? path : path + privateKey;

            // setup conditions
            var isPublic = Directory.Exists(path);
            var isPrivate = Directory.Exists(userPath);

            // NotFound conditions
            if (!isPublic && !isPrivate)
            {
                _logger?.Debug($"Get-Plugins -Path {path} -UserPath {userPath} = (NotFound | Path | UserPath)");
                return Array.Empty<string>();
            }

            // collect plugins
            var plugins = new List<string>();
            if (isPublic)
            {
                var collection = Directory
                    .GetDirectories(path)
                    .SelectMany(Directory.GetFiles)
                    .Select(File.ReadAllText);
                plugins.AddRange(collection);
            }
            if (isPrivate && !path.Equals(userPath, StringComparison.OrdinalIgnoreCase))
            {
                var collection = Directory
                    .GetDirectories(userPath)
                    .SelectMany(Directory.GetFiles)
                    .Select(File.ReadAllText);
                plugins.AddRange(collection);
            }

            // results
            _logger?.Debug($"Get-Plugins -Path {path} -UserPath {userPath} = (OK, {plugins.Count})");
            return plugins;
        }

        /// <summary>
        /// Exports the entire `Plugins` folder as a ZIP archive.
        /// </summary>
        /// <returns>File stream result</returns>
        public (int StatusCode, Stream Stream) ExportPlugins()
        {
            // constants
            var fileName = $"{Guid.NewGuid()}.zip";
            var tempFolder = Path.GetTempPath();

            try
            {
                // setup
                var pluginsDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");

                // not found
                if (!Directory.Exists(pluginsDirectory))
                {
                    return (StatusCodes.Status404NotFound, default);
                }

                // create archive
                var filePath = Path.Combine(tempFolder, fileName);
                ZipFile.CreateFromDirectory(pluginsDirectory, filePath, CompressionLevel.Optimal, false);

                // read into stream
                var bytes = File.ReadAllBytes(filePath);
                var memoryStream = new MemoryStream(bytes);

                // clean
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception e) when(e!=null)
                {
                    _logger.Warn(e.GetBaseException().Message);
                }

                // get
                return (StatusCodes.Status200OK, memoryStream);
            }
            catch (Exception e) when (e != null)
            {
                _logger.Error(e.GetBaseException().Message, e);
                return (StatusCodes.Status500InternalServerError, default);
            }
        }
        #endregion

        #region *** Update ***
        /// <summary>
        /// Add a new plugin to the domain state.
        /// </summary>
        /// <param name="id">The id of the environment to add plugin to.</param>
        /// <param name="entity">The RhinoPlugin object to post.</param>
        /// <returns><see cref="int"/> and KeyValuePair<string, object> object (if any).</returns>
        public override (int StatusCode, string Entity) Update(string id, string entity)
        {
            _logger?.Debug($"Update-Plugin -Id {id} = (NotImplemented, NotSuiteable)");
            return (StatusCodes.Status200OK, string.Empty);
        }
        #endregion
    }
}
