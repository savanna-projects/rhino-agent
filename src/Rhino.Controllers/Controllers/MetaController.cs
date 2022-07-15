/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;

using Swashbuckle.AspNetCore.Annotations;

using System.Net.Mime;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        // constants
        private static readonly string[] s_excludeActions = new[]
        {
            ActionType.Assert,
            ActionType.BannersListener,
            ActionType.Condition,
            ActionType.ExtractData,
            ActionType.Repeat
        };

        // members: state
        private readonly IMetaDataRepository _dataRepository;

        // members: private properties
        private Authentication Authentication => Request.GetAuthentication();

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="dataRepository">An IStaticDataRepository implementation to use with the Controller.</param>
        public MetaController(IMetaDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        #region *** Get    ***
        // GET: api/v3/meta/plugins/references
        [HttpGet, Route("plugins/references")]
        [SwaggerOperation(
            Summary = "Get-Plugin -All -Names",
            Description = "Returns a list of available _**Plugins**_ (both _**Rhino**_ and _**Code**_).")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ActionModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetPluginsReferences()
        {
            // get response
            var entities = _dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins()
                .Where(i => !s_excludeActions.Contains(i.Key))
                .Select(i => new
                {
                    i.Key,
                    Literal = i.Key.PascalToSpaceCase(),
                    i.Source,
                    Description = i.Entity.GetType().GetProperty("Description")?.GetValue(i.Entity) ?? string.Empty,
                    Aliases = Array.Empty<string>()
                });

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/plugins/references/:key
        [HttpGet, Route("plugins/references/{key}")]
        [SwaggerOperation(
            Summary = "Get-Plugin -key {pluginKey}",
            Description = "Returns a single available _**Plugin**_ (both _**Rhino**_ and _**Code**_).")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(ActionModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetPluginsReferences([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins()
                .FirstOrDefault(i => !s_excludeActions.Contains(i.Key) && i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Plugin -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(new
            {
                entity.Key,
                Literal = entity.Key.PascalToSpaceCase(),
                entity.Source,
                Description = entity.Entity.GetType().GetProperty("Description")?.GetValue(entity.Entity) ?? string.Empty,
                Aliases = Array.Empty<string>()
            });
        }

        // GET: api/v3/meta/plugins/references/:key/configurations/:configuration
        [HttpGet, Route("plugins/references/{key}/configurations/{configuration}")]
        [SwaggerOperation(
            Summary = "Get-Plugin -All -Names -Configuration 6846d0b7-839a-404f-a7f4-f927dc168e0c -Key {pluginKey}",
            Description = "Returns a single available _**Plugins**_ of any type (_**Rhino**_, _**Code**_ & _**External**_).")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ActionModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetPluginsReferencesByConfiguration(
            [SwaggerParameter(SwaggerDocument.Parameter.Congifuration)] string configuration,
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins(configuration)
                .FirstOrDefault(i => !s_excludeActions.Contains(i.Key) && i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Plugin -Configuration {configuration} -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(new
            {
                entity.Key,
                Literal = entity.Key.PascalToSpaceCase(),
                entity.Source,
                Description = entity.Entity.GetType().GetProperty("Description")?.GetValue(entity.Entity) ?? string.Empty,
                Aliases = Array.Empty<string>()
            });
        }

        // GET: api/v3/meta/plugins/references/configurations/:configuration
        [HttpGet, Route("plugins/references/configurations/{configuration}")]
        [SwaggerOperation(
            Summary = "Get-Plugin -All -Names -Configuration 6846d0b7-839a-404f-a7f4-f927dc168e0c",
            Description = "Returns a list of available _**Plugins**_ of all types (_**Rhino**_, _**Code**_ & _**External**_).")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ActionModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetPluginsReferencesByConfiguration([SwaggerParameter(SwaggerDocument.Parameter.Congifuration)] string configuration)
        {
            // get response
            var entities = _dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins(configuration)
                .Where(i => !s_excludeActions.Contains(i.Key))
                .Select(i => new
                {
                    i.Key,
                    Literal = i.Key.PascalToSpaceCase(),
                    i.Source,
                    Description = i.Entity.GetType().GetProperty("Description")?.GetValue(i.Entity) ?? string.Empty,
                    Aliases = Array.Empty<string>()
                });

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/plugins
        [HttpGet, Route("plugins")]
        [SwaggerOperation(
            Summary = "Get-Plugin -All",
            Description = "Returns a list of available _**Plugins**_ (both _**Rhino**_ and _**Code**_).")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ActionModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetPlugins()
        {
            // get response
            var entities = _dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins()
                .Where(i => !s_excludeActions.Contains(i.Key));

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/plugins/:key
        [HttpGet, Route("plugins/{key}")]
        [SwaggerOperation(
            Summary = "Get-Plugin -key {pluginKey}",
            Description = "Returns a single available _**Plugin**_ (both _**Rhino**_ and _**Code**_).")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(ActionModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetPlugins([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins()
                .FirstOrDefault(i => !s_excludeActions.Contains(i.Key) && i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Plugin -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/plugins/:key/configurations/:configuration
        [HttpGet, Route("plugins/{key}/configurations/{configuration}")]
        [SwaggerOperation(
            Summary = "Get-Plugin -All -Configuration 6846d0b7-839a-404f-a7f4-f927dc168e0c -Key {pluginKey}",
            Description = "Returns a single available _**Plugins**_ of any type (_**Rhino**_, _**Code**_ & _**External**_).")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ActionModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetPluginsByConfiguration(
            [SwaggerParameter(SwaggerDocument.Parameter.Congifuration)] string configuration,
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins(configuration)
                .FirstOrDefault(i => !s_excludeActions.Contains(i.Key) && i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Plugin -Configuration {configuration} -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/plugins/configurations/:configuration
        [HttpGet, Route("plugins/configurations/{configuration}")]
        [SwaggerOperation(
            Summary = "Get-Plugin -All -Configuration 6846d0b7-839a-404f-a7f4-f927dc168e0c",
            Description = "Returns a list of available _**Plugins**_ of all types (_**Rhino**_, _**Code**_ & _**External**_).")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ActionModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetPluginsByConfiguration([SwaggerParameter(SwaggerDocument.Parameter.Congifuration)] string configuration)
        {
            // get response
            var entities = _dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins(configuration)
                .Where(i => !s_excludeActions.Contains(i.Key));

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/assertions
        [HttpGet, Route("assertions")]
        [SwaggerOperation(
            Summary = "Get-Assertion -All",
            Description = "Returns a list of available _**Assertions**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<AssertModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetAssertions()
        {
            // get response
            var entities = _dataRepository.SetAuthentication(Authentication).GetAssertions();

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/assertions/:key
        [HttpGet, Route("assertions/{key}")]
        [SwaggerOperation(
            Summary = "Get-Assertion -key {assertionKey}",
            Description = "Returns a single available _**Assertion**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(AssertModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetAssertions([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetAssertions()
                .FirstOrDefault(i => i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Assertion -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/connectors
        [HttpGet, Route("connectors")]
        [SwaggerOperation(
            Summary = "Get-Connector -All",
            Description = "Returns a list of available _**Connectors**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ConnectorModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetConnectors()
        {
            // get response
            var entities = _dataRepository.SetAuthentication(Authentication).GetConnectors();

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/connectors/:key
        [HttpGet, Route("connectors/{key}")]
        [SwaggerOperation(
            Summary = "Get-Connector -key {connectorKey}",
            Description = "Returns a single available _**Connector**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(ConnectorModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetConnectors([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetConnectors()
                .FirstOrDefault(i => i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Connector -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/drivers
        [HttpGet, Route("drivers")]
        [SwaggerOperation(
            Summary = "Get-Driver -All",
            Description = "Returns a list of available _**Drivers**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<DriverModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetDrivers()
        {
            // get response
            var entities = _dataRepository.SetAuthentication(Authentication).GetDrivers();

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/drivers/:key
        [HttpGet, Route("drivers/{key}")]
        [SwaggerOperation(
            Summary = "Get-Driver -key {driverKey}",
            Description = "Returns a single available _**Driver**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(DriverModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetDrivers([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetDrivers()
                .FirstOrDefault(i => i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Driver -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/locators
        [HttpGet, Route("locators")]
        [SwaggerOperation(
            Summary = "Get-Locator -All",
            Description = "Returns a list of available _**Locators**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<LocatorModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetLocators()
        {
            // get response
            var entities = _dataRepository.SetAuthentication(Authentication).GetLocators();

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/locators/:key
        [HttpGet, Route("locators/{key}")]
        [SwaggerOperation(
            Summary = "Get-Locator -key {locatorKey}",
            Description = "Returns a single available _**Locator**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(LocatorModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetLocators([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetLocators()
                .FirstOrDefault(i => i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Locator -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/macros
        [HttpGet, Route("macros")]
        [SwaggerOperation(
            Summary = "Get-Macro -All",
            Description = "Returns a list of available _**Macros**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<MacroModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetMacros()
        {
            // get response
            var entities = _dataRepository.SetAuthentication(Authentication).GetMacros();

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/macros/:key
        [HttpGet, Route("macros/{key}")]
        [SwaggerOperation(
            Summary = "Get-Macro -key {macroKey}",
            Description = "Returns a single available _**Macro**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(MacroModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetMacros([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetMacros()
                .FirstOrDefault(i => i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Macro -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/operators
        [HttpGet, Route("operators")]
        [SwaggerOperation(
            Summary = "Get-Operator -All",
            Description = "Returns a list of available _**Operators**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<OperatorModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetOperators()
        {
            // get response
            var entities = _dataRepository.SetAuthentication(Authentication).GetOperators();

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/operators/:key
        [HttpGet, Route("operators/{key}")]
        [SwaggerOperation(
            Summary = "Get-Operator -key {operatorKey}",
            Description = "Returns a single available _**Operator**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(OperatorModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetOperators([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetOperators()
                .FirstOrDefault(i => i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Operator -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/reporters
        [HttpGet, Route("reporters")]
        [SwaggerOperation(
            Summary = "Get-Reporter -All",
            Description = "Returns a list of available _**Reporters**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<OperatorModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetReporters()
        {
            // get response
            var entities = _dataRepository.SetAuthentication(Authentication).GetReporters();

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/reporters/:key
        [HttpGet, Route("reporters/{key}")]
        [SwaggerOperation(
            Summary = "Get-Reporter -key {reporterKey}",
            Description = "Returns a single available _**Reporter**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(OperatorModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetReporters([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetReporters()
                .FirstOrDefault(i => i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Reporter -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/annotations
        [HttpGet, Route("annotations")]
        [SwaggerOperation(
            Summary = "Get-Annotation -All",
            Description = "Returns a list of available _**Annotations**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<PropertyModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetAnnotations()
        {
            // get response
            var entities = _dataRepository.SetAuthentication(Authentication).GetAnnotations();

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/annotations/:key
        [HttpGet, Route("annotations/{key}")]
        [SwaggerOperation(
            Summary = "Get-Annotation -key {propertyKey}",
            Description = "Returns a single available _**Annotation**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(PropertyModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetAnnotations([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetAnnotations()
                .FirstOrDefault(i => i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Property -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/attributes
        [HttpGet, Route("attributes")]
        [SwaggerOperation(
            Summary = "Get-Attribute -All",
            Description = "Returns a list of available _**Element special attributes**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<AttributeModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetAttributes()
        {
            var model = new AttributeModel
            {
                Key = "html",
                Literal = "html",
                Verb = "from",
                Entity = new
                {
                    Name = "html",
                    Description = "Gets the outer HTML of the element."
                }
            };
            return Ok(new[] { model });
        }

        // GET: api/v3/meta/version
        [HttpGet, Route("version")]
        [SwaggerOperation(
            Summary = "Get-Version",
            Description = "Returns Rhino Server version number.")]
        [Produces(MediaTypeNames.Text.Plain, MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetVersion()
        {
            // get response
            var version = await _dataRepository.GetVersionAsync().ConfigureAwait(false);

            // return
            return Ok(version);
        }

        // GET: api/v3/meta/models
        [HttpGet, Route("models")]
        [SwaggerOperation(
            Summary = "Get-Models -All",
            Description = "Returns a list of available _**Rhino Page Models**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<RhinoModelCollection>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetModels()
        {
            // setup
            var entities = _dataRepository.SetAuthentication(Authentication).GetModels();

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/models/:name
        [HttpGet, Route("models/{name}")]
        [SwaggerOperation(
            Summary = "Get-Models -Name {the name of the model}",
            Description = "Returns a single available _**Rhino Page Model**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoPageModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetModels([SwaggerParameter(SwaggerDocument.Parameter.Id)] string name)
        {
            // setup
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetModels()
                .SelectMany(i => i.Models)
                .FirstOrDefault(i => i.Name.Equals(name, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Model -Name {name} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // get
            return Ok(entity);
        }

        // GET: api/v3/meta/models/types
        [HttpGet, Route("models/types")]
        [SwaggerOperation(
            Summary = "Get-ModelTypes",
            Description = "Returns a collection of all available _**Rhino Model Types**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<RhinoModelTypeModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetModelTypes()
        {
            // setup
            var entity = _dataRepository.SetAuthentication(Authentication).GetModelTypes();

            // get
            return Ok(entity);
        }

        // GET: api/v3/meta/verbs
        [HttpGet, Route("verbs")]
        [SwaggerOperation(
            Summary = "Get-Verbs -Name All",
            Description = "Returns a collection of all available _**Rhino Verbs**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<RhinoVerbModel>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetVerbs()
        {
            // setup
            var entities = _dataRepository
                .SetAuthentication(Authentication)
                .GetVerbs();

            // not found
            if (entities?.Any() == false)
            {
                return await this
                    .ErrorResultAsync<string>("Get-Verbs -Name All = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // get
            return Ok(entities);
        }

        // GET: api/v3/meta/events
        [HttpGet, Route("events")]
        [SwaggerOperation(
            Summary = "Get-ServiceEvent -All",
            Description = "Returns a list of available _**Service Events**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ServiceEventModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetServiceEvents()
        {
            // get response
            var entities = _dataRepository.SetAuthentication(Authentication).GetServiceEvents();

            // return
            return Ok(entities);
        }

        // GET: api/v3/meta/events/:key
        [HttpGet, Route("events/{key}")]
        [SwaggerOperation(
            Summary = "Get-ServiceEvent -Key {eventKey}",
            Description = "Returns a single available _**Service Event**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(ServiceEventModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetServiceEvents([SwaggerParameter(SwaggerDocument.Parameter.Id)] string key)
        {
            // get response
            var entity = _dataRepository
                .SetAuthentication(Authentication)
                .GetServiceEvents()
                .FirstOrDefault(i => i.Key.Equals(key, StringComparison.Ordinal));

            // not found
            if (entity == default)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-ServiceEvent -Key {key} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(entity);
        }

        // GET: api/v3/meta/services
        [HttpGet, Route("services")]
        [SwaggerOperation(
            Summary = "Get-Services -All",
            Description = "list of all available micro services under _**Rhino.Controllers.dll**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(ServiceEventModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetServices()
        {
            // build
            var services = _dataRepository.GetServices();

            // get
            return Ok(services);
        }
        #endregion

        #region *** Post   ***
        // GET: api/v3/meta/tests/tree
        [HttpPost, Route("tests/tree")]
        [SwaggerOperation(
            Summary = "Get-TestTree",
            Description = "Gets an ASCII tree based on the RhinoTestCase spec provided.")]
        [Consumes(MediaTypeNames.Text.Plain)]
        [Produces(MediaTypeNames.Text.Plain)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult GetTestTree([FromBody] string rhinoTestCase)
        {
            // get response
            var entities = _dataRepository
                .SetAuthentication(Authentication)
                .GetTestTree(rhinoTestCase);

            // return
            return Ok(entities);
        }
        #endregion
    }
}
