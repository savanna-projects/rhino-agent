/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using Microsoft.AspNetCore.Http;

using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Models.Server;

namespace Rhino.Controllers.Domain.Automation
{
    /// <summary>
    /// Data Access Layer for Rhino API resources repository.
    /// </summary>
    public class ResourcesRepository : IResourcesRepository
    {
        // constants
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;
        private const string ResourcePath = "Resources";
        private static readonly ReaderWriterLock s_readerWriterLock = new();
        private static readonly TimeSpan s_timeout = TimeSpan.FromMinutes(1);

        // members: state
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="logger">An ILogger implementation to use with the Repository.</param>
        public ResourcesRepository(ILogger logger)
        {
            _logger = logger.CreateChildLogger(nameof(ResourcesRepository));
        }

        #region *** Create ***
        /// <summary>
        /// Create or replace a resource file into the resources folder.
        /// </summary>
        /// <param name="entity">The ResourceFileModel object to post.</param>
        /// <returns>The status code and the ResourceFileModel.</returns>
        public (int StatusCode, ResourceFileModel Entity) Create(ResourceFileModel entity)
        {
            return WriteResource(entity, timeout: s_timeout, logger: _logger);
        }
        #endregion

        #region *** Delete ***
        /// <summary>
        /// Deletes all resources from the resources folder.
        /// </summary>
        /// <returns>Code 204, NoContent</returns>
        public int Delete()
        {
            // delete
            try
            {
                s_readerWriterLock.AcquireWriterLock(s_timeout);
                if (Directory.Exists(path: ResourcePath))
                {
                    Directory.Delete(path: ResourcePath, recursive: true);
                }
            }
            catch (Exception e) when (e != null)
            {
                _logger.Error(e.Message, e);
                return StatusCodes.Status500InternalServerError;
            }
            finally
            {
                s_readerWriterLock.ReleaseWriterLock();
            }

            // get
            return StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Deletes all resources by resource id.
        /// </summary>
        /// <param name="id">The file name including extension.</param>
        /// <returns>Code 204, NoContent</returns>
        public int Delete(string id)
        {
            // setup
            var path = Directory.Exists(ResourcePath)
                ? Array.Find(Directory.GetFiles(ResourcePath), i => i.EndsWith(id, Compare))
                : string.Empty;

            // no files
            if (string.IsNullOrEmpty(path))
            {
                return StatusCodes.Status404NotFound;
            }

            // delete
            Delete(path, s_timeout, _logger);

            // get
            return StatusCodes.Status204NoContent;
        }

        private static void Delete(string path, TimeSpan timeout, ILogger logger)
        {
            try
            {
                // get lock
                s_readerWriterLock.AcquireWriterLock(timeout);

                // delete
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e) when (e != null)
            {
                logger.Error(e.Message, e);
            }
            finally
            {
                s_readerWriterLock.ReleaseWriterLock();
            }
        }
        #endregion

        #region *** Get    ***
        /// <summary>
        /// Gets a collection of ResourceFileModel with all the resources available under resources path.
        /// </summary>
        /// <returns>A collection of ResourceFileModel.</returns>
        public IEnumerable<ResourceFileModel> Get()
        {
            // setup
            var files = Directory.Exists(ResourcePath)
                ? Directory.GetFiles(ResourcePath)
                : Array.Empty<string>();

            // no files
            if (files.Length == 0)
            {
                return Array.Empty<ResourceFileModel>();
            }

            // iterate
            var contents = files.Select(i => new ResourceFileModel
            {
                Content = Get(i, s_timeout, _logger),
                FileName = Path.GetFileName(i),
                Path = Path.Combine(ResourcePath, Path.GetFileName(i))
            });

            // get
            return contents
                .Where(i => !string.IsNullOrEmpty(i.Content))
                .OrderBy(i => i.Path);
        }

        /// <summary>
        /// Get a ResourceFileModel by the resource id.
        /// </summary>
        /// <param name="id">The resource id (file name).</param>
        /// <returns>A ResourceFileModel.</returns>
        public (int StatusCode, ResourceFileModel Entity) Get(string id)
        {
            // setup
            var file = Directory.Exists(ResourcePath)
                ? Array.Find(Directory.GetFiles(ResourcePath), i => i.EndsWith(id, Compare))
                : string.Empty;

            // no files
            if (string.IsNullOrEmpty(file))
            {
                return (StatusCodes.Status404NotFound, new ResourceFileModel());
            }

            // read
            var entity = new ResourceFileModel
            {
                Content = Get(file, s_timeout, _logger),
                FileName = id,
                Path = file
            };

            // get
            return (StatusCodes.Status200OK, entity);
        }

        private static string Get(string path, TimeSpan timeout, ILogger logger)
        {
            try
            {
                // get lock
                s_readerWriterLock.AcquireReaderLock(timeout);

                // get
                return File.Exists(path)
                    ? File.ReadAllText(path)
                    : string.Empty;
            }
            catch (Exception e) when (e != null)
            {
                logger.Error(e.Message, e);
                return string.Empty;
            }
            finally
            {
                s_readerWriterLock.ReleaseReaderLock();
            }
        }
        #endregion

        // Utilities
        private static (int StatusCode, ResourceFileModel Entity) WriteResource(
            ResourceFileModel entity,
            TimeSpan timeout,
            ILogger logger)
        {
            // bad request
            if (string.IsNullOrEmpty(entity?.FileName))
            {
                return (StatusCodes.Status400BadRequest, new ResourceFileModel());
            }

            try
            {
                // get lock
                s_readerWriterLock.AcquireWriterLock(timeout);

                // write file
                Directory.CreateDirectory(ResourcePath);
                var path = Path.Combine(ResourcePath, entity.FileName);
                File.WriteAllText(path, contents: entity.Content);

                // get
                return (StatusCodes.Status201Created, entity);
            }
            catch (Exception e) when (e != null)
            {
                logger.Error(e.Message, e);
                return (StatusCodes.Status500InternalServerError, null);
            }
            finally
            {
                s_readerWriterLock.ReleaseWriterLock();
            }
        }
    }
}
