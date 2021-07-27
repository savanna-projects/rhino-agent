/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        // constants
        private static readonly string[] ExcludeActions = new[]
        {
            ActionType.Assert,
            ActionType.BannersListener,
            ActionType.Condition,
            ActionType.ExtractData,
            ActionType.Repeat
        };

        // members: state
        private readonly IMetaDataRepository dataRepository;

        // members: private properties
        private Authentication Authentication => Request.GetAuthentication();

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="dataRepository">An IStaticDataRepository implementation to use with the Controller.</param>
        public MetaController(IMetaDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
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
            var entities = dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins()
                .Where(i => !ExcludeActions.Contains(i.Key))
                .Select(i => new
                {
                    i.Key,
                    Literal = i.Key.PascalToSpaceCase(),
                    i.Source,
                    Description = i.Entity.GetType().GetProperty("Description").GetValue(i.Entity),
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
            var entity = dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins()
                .FirstOrDefault(i => !ExcludeActions.Contains(i.Key) && i.Key.Equals(key, StringComparison.Ordinal));

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
                Description = entity.Entity.GetType().GetProperty("Description").GetValue(entity.Entity),
                Aliases = Array.Empty<string>()
            });
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
            var entities = dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins()
                .Where(i => !ExcludeActions.Contains(i.Key));

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
            var entity = dataRepository
                .SetAuthentication(Authentication)
                .GetPlugins()
                .FirstOrDefault(i => !ExcludeActions.Contains(i.Key) && i.Key.Equals(key, StringComparison.Ordinal));

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
            var entities = dataRepository.SetAuthentication(Authentication).GetAssertions();

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
            var entity = dataRepository
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
            var entities = dataRepository.SetAuthentication(Authentication).GetConnectors();

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
            var entity = dataRepository
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
            var entities = dataRepository.SetAuthentication(Authentication).GetDrivers();

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
            var entity = dataRepository
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
            var entities = dataRepository.SetAuthentication(Authentication).GetLocators();

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
            var entity = dataRepository
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
            var entities = dataRepository.SetAuthentication(Authentication).GetMacros();

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
            var entity = dataRepository
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
            var entities = dataRepository.SetAuthentication(Authentication).GetOperators();

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
            var entity = dataRepository
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
            var entities = dataRepository.SetAuthentication(Authentication).GetReporters();

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
            var entity = dataRepository
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
            var entities = dataRepository.SetAuthentication(Authentication).GetAnnotations();

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
            var entity = dataRepository
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
            var version = await dataRepository.GetVersionAsync().ConfigureAwait(false);

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
            var entities = dataRepository.SetAuthentication(Authentication).GetModels();

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
            var entity = dataRepository
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

        // GET: api/v3/meta/verbs
        [HttpGet, Route("verbs")]
        [SwaggerOperation(
            Summary = "Get-Verbs -Name All",
            Description = "Returns a collection of all available _**Rhino Verbs**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<RhinoVerbModel>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetVerbs([SwaggerParameter(SwaggerDocument.Parameter.Id)] string name)
        {
            // setup
            var entities = dataRepository
                .SetAuthentication(Authentication)
                .GetVerbs();

            // not found
            if (entities?.Any() == false)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-Verbs -Name All = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // get
            return Ok(entities);
        }
        #endregion
    }
}