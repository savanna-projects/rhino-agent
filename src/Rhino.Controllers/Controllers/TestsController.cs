/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts;
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

using ILogger = Gravity.Abstraction.Logging.ILogger;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        // members: state
        private readonly IDomain _domain;
        private readonly ILogger _logger;

        // members: private properties
        private Authentication Authentication => Request.GetAuthentication();

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="domain">An IDomain implementation to use with the Controller.</param>
        /// <param name="logger">An ILogger implementation to use with the Controller.</param>
        public TestsController(IDomain domain, ILogger logger)
        {
            _domain = domain;
            _logger = logger;
        }

        #region *** Get    ***
        // GET: api/v3/tests
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get-TestCollection -All",
            Description = "Returns a list of available _**Rhino Test Cases**_ collections.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<TestResponseModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Get()
        {
            // setup
            var responseBody = _domain.Tests.SetAuthentication(Authentication).Get().Select(i => new TestResponseModel
            {
                Id = $"{i.Id}",
                Configurations = i.Configurations,
                Tests = i.RhinoTestCaseModels.Count
            });

            // add count header
            Response.Headers.Add(RhinoResponseHeader.CountTotalSpecs, $"{responseBody.Select(i => i.Tests).Sum()}");

            // response
            return Ok(responseBody);
        }

        // GET api/v3/tests/:id
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get-TestCollection -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Returns an existing _**Rhino Test Case**_ collection (test suite content).")]
        [Produces(MediaTypeNames.Text.Plain)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public Task<IActionResult> Get([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            return InvokeGet(id);
        }

        // GET api/v3/tests/:id/configurations
        [HttpGet("{id}/configurations")]
        [SwaggerOperation(
            Summary = "Get-TestCollection -Configuration -All -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Returns a list of available _**Rhino Configurations**_ which are associated with this _**Rhino Test Case**_ collection.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetConfigurations([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // setup
            var (statusCode, entity) = _domain.Tests.SetAuthentication(Authentication).Get(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-TestCollection -Configuration -All -Id {id} = (NotFound, TestCollection)", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // get
            return entity.Configurations.Count == 0 ? Ok(Array.Empty<string>()) : Ok(entity.Configurations);
        }
        #endregion

        #region *** Post   ***
        // POST api/v3/tests
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create-TestCollection",
            Description = "Creates a new _**Rhino Test Case Collection**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(TestResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public Task<IActionResult> Create()
        {
            return DoCreate(configuration: string.Empty);
        }

        // POST api/v3/tests/:configuration
        [HttpPost("{configuration}")]
        [SwaggerOperation(
            Summary = "Create-TestCollection -Configuration {00000000-0000-0000-0000-000000000000}",
            Description = "Creates a new _**Rhino Test Case Collection**_ and attach it to the provided configuration.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(TestResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public Task<IActionResult> Create([SwaggerParameter(SwaggerDocument.Parameter.Congifuration)] string configuration)
        {
            return DoCreate(configuration);
        }

        private async Task<IActionResult> DoCreate(string configuration)
        {
            // setup
            var tests = await Request.ReadAsync().ConfigureAwait(false);
            var documents = tests.Split(Utilities.Separator);
            var collection = new RhinoTestCollection();

            // parse test cases
            collection.RhinoTestCaseModels = documents
                .Select(i => new RhinoTestModel { Collection = $"{collection.Id}", Id = Guid.NewGuid(), RhinoSpec = i })
                .Where(i => !string.IsNullOrEmpty(i.RhinoSpec))
                .ToList();

            // create id for this collection
            var id = _domain.Tests.SetAuthentication(Authentication).Add(collection);
            AddConfiguration(onCollection: collection, configuration);

            // build
            Response.Headers.Add(RhinoResponseHeader.CountTotalSpecs, $"{collection.RhinoTestCaseModels.Count}");
            var responseBody = new TestResponseModel
            {
                Id = id,
                Configurations = new[] { configuration },
                Tests = collection.RhinoTestCaseModels.Count
            };

            // get
            return Created($"api/v3/tests/{id}", responseBody);
        }

        private void AddConfiguration(RhinoTestCollection onCollection, string configuration)
        {
            // setup
            onCollection.Configurations ??= new List<string>();

            // no configuration
            if (string.IsNullOrEmpty(configuration))
            {
                _logger.Debug($"Create-TestCollection -Configuration {configuration} = (BadRequest, NoConfiguration)");
                return;
            }

            // check configuration
            var (statusCode, onConfiguration) = _domain.Configurations.SetAuthentication(Authentication).Get(configuration);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                _logger.Debug($"Create-TestCollection -Configuration {configuration} = (NotFound, NoConfiguration)");
                return;
            }

            // add
            onCollection.Configurations.Add(configuration);
            _domain.Tests.SetAuthentication(Authentication).Update($"{onCollection.Id}", onCollection);

            // cascade
            onConfiguration.TestsRepository = new List<string>(onConfiguration.TestsRepository)
            {
                $"{onCollection.Id}"
            };
            _domain.Configurations.SetAuthentication(Authentication).Update(configuration, onConfiguration);
        }
        #endregion

        #region *** Patch  ***
        // PATCH api/v3/tests/:id
        [HttpPatch("{id}")]
        [SwaggerOperation(
            Summary = "Add-TestCollection -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Add additional _**Rhino Test Cases**_ into an existing collection.")]
        [Consumes(MediaTypeNames.Text.Plain)]
        [Produces(MediaTypeNames.Text.Plain)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> AddTestCases([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // read test case from request body
            var requestBody = await Request.ReadAsync().ConfigureAwait(false);
            var specs = requestBody.Split(RhinoSpecification.Separator).Select(i => i.Trim());

            // get collection
            var (statusCode, collection) = _domain.Tests.SetAuthentication(Authentication).Get(id);

            // bad request
            if (statusCode == StatusCodes.Status400BadRequest)
            {
                return await this
                    .ErrorResultAsync<string>($"Add-TestCollection -Id {id} = (BadRequest, NoTests)")
                    .ConfigureAwait(false);
            }

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Add-TestCollection -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // build
            var testCaseModels = specs.Select(i => new RhinoTestModel
            {
                Collection = id,
                Id = Guid.NewGuid(),
                RhinoSpec = i
            });
            collection.RhinoTestCaseModels = collection.RhinoTestCaseModels.Concat(testCaseModels).ToList();

            // update
            _domain.Tests.SetAuthentication(Authentication).Update($"{collection.Id}", collection);

            // get
            return await InvokeGet(id).ConfigureAwait(false);
        }

        // PATCH api/v3/tests/:id/configurations/:configuration
        [HttpPatch("{id}/configurations/{configuration}")]
        [SwaggerOperation(
            Summary = "Add-TestCollection -Id {00000000-0000-0000-0000-000000000000} -Configuration {00000000-0000-0000-0000-000000000000}",
            Description = "Add additional _**Rhino Configuration**_ into an existing collection.")]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<string>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> AddConfiguration(
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string id,
            [SwaggerParameter(SwaggerDocument.Parameter.Congifuration)] string configuration)
        {
            // patch
            var (statusCode, _) = ((ITestsRepository)_domain.Tests.SetAuthentication(Authentication)).Update(id, configuration);

            // bad request
            if (statusCode == StatusCodes.Status400BadRequest)
            {
                var message = "Add-TestConfiguration" +
                    $"-TestCollection {id}" +
                    $"-Configuration {configuration} = (BadRequest, NoConfiguration)";

                return await this
                    .ErrorResultAsync<string>(message, StatusCodes.Status400BadRequest)
                    .ConfigureAwait(false);
            }

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                var message = "Add-TestConfiguration" +
                    $"-TestCollection {id}" +
                    $"-Configuration {configuration} = (NoFound, TestCaseCollection | RhinoConfiguration)";

                return await this
                    .ErrorResultAsync<string>(message, StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // get
            var entity = _domain.Tests.SetAuthentication(Authentication).Get(id);
            return new ContentResult
            {
                Content = entity.Entity.Configurations.ToJson(),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = entity.StatusCode
            };
        }
        #endregion

        #region *** Delete ***
        // DELETE api/v3/tests/:id
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Delete-TestCollection -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Deletes an existing _**Rhino Test Case**_ collection.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Delete([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // delete
            var statusCode = _domain.Tests.SetAuthentication(Authentication).Delete(id);

            // results
            return statusCode == StatusCodes.Status404NotFound
                ? await this.ErrorResultAsync<string>($"Delete-TestCollection -id {id} = NotFound", statusCode).ConfigureAwait(false)
                : NoContent();
        }

        // DELETE api/v3/tests
        [HttpDelete]
        [SwaggerOperation(
            Summary = "Delete-TestCollection -All",
            Description = "Deletes all existing _**Rhino Test Case**_ collections.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Delete()
        {
            // execute
            _domain.Tests.SetAuthentication(Authentication).Delete();

            // get
            return NoContent();
        }
        #endregion

        // Utilities
        private async Task<IActionResult> InvokeGet(string id)
        {
            // setup
            var (statusCode, entity) = _domain.Tests.SetAuthentication(Authentication).Get(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-TestCollection -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // setup
            var specs = entity.RhinoTestCaseModels.Select(i => i.RhinoSpec);
            var responseBody = string.Join(Utilities.Separator, specs);

            // add count header
            Response.Headers.Add(RhinoResponseHeader.CountTotalSpecs, $"{specs.Count()}");

            // get
            return Ok(responseBody);
        }
    }
}
