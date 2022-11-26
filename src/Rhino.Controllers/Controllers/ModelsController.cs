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
using Rhino.Controllers.Domain;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion($"{AppSettings.ApiVersion}.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public partial class ModelsController : ControllerBase
    {
        #region *** Expressions ***
        [GeneratedRegex("(\\s+)?/\\*\\*.*")]
        private static partial Regex GetEmptyOrCommentToken();

        [GeneratedRegex("(?<=\\[test-models\\]).*")]
        private static partial Regex GetModelsToken();
        #endregion

        // constants
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

        // members: state
        private readonly IDomain _domain;

        // members: private properties
        private Authentication Authentication => Request.GetAuthentication();

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="domain">An IDomain implementation to use with the Controller.</param>
        public ModelsController(IDomain domain)
        {
            _domain = domain;
        }

        #region *** Get    ***
        // GET: api/v3/models
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get-RhinoModelCollection -All",
            Description = "Returns a list of available _**Rhino Models**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<ModelCollectionResponseModel>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Get()
        {
            // setup
            var responseBody = _domain
                .Models
                .SetAuthentication(Authentication)
                .Get()
                .Select(GetCollection);

            // response
            return Ok(responseBody);
        }

        // GET api/v3/models/:id
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get-RhinoModelCollection -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Returns an existing _**Rhino Model**_ collection.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoModelCollection))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Get([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get data
            var (statusCode, modelCollection) = _domain.Models.SetAuthentication(Authentication).Get(id);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-RhinoModelCollection -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(modelCollection);
        }

        // GET api/v3/models/:id/configurations
        [HttpGet("{id}/configurations")]
        [SwaggerOperation(
            Summary = "Get-RhinoModelCollection -Configuration -All -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Returns a list of available _**Rhino Configurations**_ which are associated with this _**Rhino Model**_ collection.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetConfigurations([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get data
            var (statusCode, modelCollection) = _domain.Models.SetAuthentication(Authentication).Get(id);

            // exit conditions
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-RhinoModelCollection -Configuration -All -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // return
            return Ok(modelCollection.Configurations);
        }

        private ModelCollectionResponseModel GetCollection(RhinoModelCollection collection) => new()
        {
            Id = $"{collection.Id}",
            Configurations = collection.Configurations,
            Models = collection.Models.Count,
            Entries = collection.Models.SelectMany(i => i.Entries).Count()
        };
        #endregion

        #region  *** Post   ***
        // POST api/v3/models
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create-RhinoModelCollection",
            Description = "Creates a new _**Rhino Model**_.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoModelCollection))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<IEnumerable<RhinoPageModel>>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<IEnumerable<RhinoPageModel>>))]
        public Task<IActionResult> Create([FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] IEnumerable<RhinoPageModel> pageModels)
        {
            return InvokeCreate(configuration: string.Empty, pageModels);
        }

        // POST api/v3/models/:configuration
        [HttpPost("{configuration}")]
        [SwaggerOperation(
            Summary = "Create-RhinoModelCollection -Configuration {00000000-0000-0000-0000-000000000000}",
            Description = "Creates a new _**Rhino Model**_ and attach it to the provided configuration.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoModelCollection))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<IEnumerable<RhinoPageModel>>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<IEnumerable<RhinoPageModel>>))]
        public Task<IActionResult> Create(
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string configuration,
            [FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] IEnumerable<RhinoPageModel> pageModels)
        {
            return InvokeCreate(configuration, pageModels);
        }

        // POST api/v3/models/md
        [HttpPost, Route("md")]
        [SwaggerOperation(
            Summary = "Create-RhinoModelCollection -Type Markdown",
            Description = "Creates a new _**Rhino Model**_.")]
        [Consumes(MediaTypeNames.Text.Plain)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoModelCollection))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public Task<IActionResult> Create([FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] string pageModels)
        {
            // build
            var _pageModels = CleanSpec(pageModels)
                .Split(">>>")
                .Select(i => FormatPageModel(i))
                .Select(i => new RhinoPageModel().GetFromMarkdown(i.Name, i.Markdown));

            // get
            return InvokeCreate(configuration: string.Empty, _pageModels);
        }

        // POST api/v3/models/md/:configuration
        [HttpPost("md/{configuration}")]
        [SwaggerOperation(
            Summary = "Create-RhinoModelCollection -Type Markdown -Configuration {00000000-0000-0000-0000-000000000000}",
            Description = "Creates a new _**Rhino Model**_ and attach it to the provided configuration.")]
        [Consumes(MediaTypeNames.Text.Plain)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoModelCollection))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<IEnumerable<RhinoPageModel>>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<IEnumerable<RhinoPageModel>>))]
        public Task<IActionResult> Create(
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string configuration,
            [FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] string pageModels)
        {
            // build
            var _pageModels = CleanSpec(pageModels)
                .Split(">>>")
                .Select(i => FormatPageModel(i))
                .Select(i => new RhinoPageModel().GetFromMarkdown(i.Name, i.Markdown));

            // get
            return InvokeCreate(configuration: configuration, _pageModels);
        }

        // TODO: remove when available from Rhino.Api
        private static string CleanSpec(string spec)
        {
            // get spec lines
            var lines = spec.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // clear comments & empty lines
            lines = lines
                .Select(i => GetEmptyOrCommentToken().Replace(i, string.Empty))
                .Where(i => !string.IsNullOrEmpty(i))
                .ToArray();

            // results
            return string.Join(Environment.NewLine, lines);
        }

        private static (string Name, string Markdown) FormatPageModel(string pageModel)
        {
            // setup
            var lines = pageModel.Trim().SplitByLines().ToArray();
            var name = lines.Length > 0 ? GetModelsToken().Match(lines[0]).Value.Trim() : string.Empty;
            var markdownLines = lines.Skip(1).Where(i => !string.IsNullOrEmpty(i.Trim())).ToArray();

            // bad request
            if (string.IsNullOrEmpty(name) || markdownLines.Length < 3)
            {
                return (string.Empty, string.Empty);
            }

            // get
            return (name, string.Join('\n', markdownLines));
        }

        private async Task<IActionResult> InvokeCreate(string configuration, IEnumerable<RhinoPageModel> pageModels)
        {
            // bad request
            var badRequest = $"Create-RhinoModelCollection -Configuration {configuration} = (BadRequest, $(Reason))";
            if (pageModels?.Any() == false)
            {
                return await this
                    .ErrorResultAsync<IEnumerable<RhinoPageModel>>(badRequest.Replace("$(Reason)", "NoModels"), StatusCodes.Status400BadRequest)
                    .ConfigureAwait(false);
            }
            if (pageModels?.SelectMany(i => i.Entries)?.Any() == false)
            {
                return await this
                    .ErrorResultAsync<IEnumerable<RhinoPageModel>>(badRequest.Replace("$(Reason)", "NoEntries"), StatusCodes.Status400BadRequest)
                    .ConfigureAwait(false);
            }

            // setup
            var collection = new RhinoModelCollection();
            collection.Configurations ??= new List<string>();
            collection.Models = pageModels?.ToList() ?? new List<RhinoPageModel>();

            // add configuration
            if (!string.IsNullOrEmpty(configuration))
            {
                collection.Configurations.Add(configuration);
            }

            // add
            var id = _domain.Models.SetAuthentication(Authentication).Add(entity: collection);

            // already created
            if (string.IsNullOrEmpty(id))
            {
                return NoContent();
            }

            // results
            return Created($"/api/v3/models/{id}", collection);
        }
        #endregion

        #region *** Patch  ***
        // PATCH api/v3/models/:id
        [HttpPatch("{id}")]
        [SwaggerOperation(
            Summary = "Add-RhinoModelCollection -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Add additional _**Rhino Models**_ into an existing collection. If the model name is already exists on another model, it will be ignored.")]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoModelCollection))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<IEnumerable<RhinoPageModel>>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<IEnumerable<RhinoPageModel>>))]
        public async Task<IActionResult> Add(
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string id,
            [FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] IEnumerable<RhinoPageModel> pageModels)
        {
            // bad request
            if (string.IsNullOrEmpty(id))
            {
                var badRequest = $"Update-RhinoModelCollection -Id {id} = (BadRequest, NoCollection)";
                return await this
                    .ErrorResultAsync<IEnumerable<RhinoPageModel>>(badRequest, StatusCodes.Status400BadRequest)
                    .ConfigureAwait(false);
            }

            // setup
            var (statusCode, modelCollection) = _domain.Models.SetAuthentication(Authentication).Get(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                var notFound = $"Update-RhinoModelCollection -Id {id} = NotFound";
                return await this
                    .ErrorResultAsync<IEnumerable<RhinoPageModel>>(notFound, StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // setup
            var models = modelCollection.Models.ToList();
            var range = pageModels.Where(i => !modelCollection.Models.Select(i => i.Name).Contains(i.Name));

            // add
            models.AddRange(range);
            modelCollection.Models = models;

            // update
            _domain.Models.Update(id, modelCollection);

            // get
            var (StatusCode, Entity) = _domain.Models.Get(id);
            return new ContentResult
            {
                StatusCode = StatusCode,
                ContentType = MediaTypeNames.Application.Json,
                Content = Extensions.ObjectExtensions.ToJson(Entity)
            };
        }

        [HttpPatch("{id}/configurations/{configuration}")]
        [SwaggerOperation(
            Summary = "Add-RhinoModelCollection -Id {00000000-0000-0000-0000-000000000000} -Configuration {00000000-0000-0000-0000-000000000000}",
            Description = "Add an existing _**Configuration**_ to the provided _**Models Collection**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(RhinoModelCollection))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<IEnumerable<RhinoPageModel>>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<IEnumerable<RhinoPageModel>>))]
        public async Task<IActionResult> Add(
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string id,
            [FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Congifuration)] string configuration)
        {
            // bad request
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(configuration))
            {
                var badRequest = "Update-RhinoModelCollection" +
                    $"-Id {id}" +
                    $"-Configuration {configuration} = (BadRequest, NoCollection | NoConfiguration)";
                return await this
                    .ErrorResultAsync<IEnumerable<RhinoPageModel>>(badRequest, StatusCodes.Status400BadRequest)
                    .ConfigureAwait(false);
            }

            // setup
            var (statusCode, modelCollection) = _domain.Models.SetAuthentication(Authentication).Get(id);
            var (configurationStatusCode, _) = _domain.Configurations.SetAuthentication(Authentication).Get(configuration);

            // not found
            if (statusCode == StatusCodes.Status404NotFound || configurationStatusCode == StatusCodes.Status404NotFound)
            {
                var notFound = "Update-RhinoModelCollection" +
                    $"-Id {id}" +
                    $"-Configuration {configuration} = (NotFound, Collection | Configuration)";
                return await this
                    .ErrorResultAsync<IEnumerable<RhinoPageModel>>(notFound, StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // add if not exists
            if (!modelCollection.Configurations.Any(i => i.Equals(configuration, Compare)))
            {
                modelCollection.Configurations.Add(configuration);
            }

            // update
            _domain.Models.Update(id, modelCollection);

            // get
            var (StatusCode, Entity) = _domain.Models.Get(id);
            return new ContentResult
            {
                StatusCode = StatusCode,
                ContentType = MediaTypeNames.Application.Json,
                Content = Extensions.ObjectExtensions.ToJson(Entity)
            };
        }
        #endregion

        #region *** Delete ***
        // DELETE api/v3/models/:id
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Delete-RhinoModelCollection -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Deletes an existing _**Rhino Model**_ collection.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(string))]
        public async Task< IActionResult> Delete([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // delete
            var statusCode = _domain.Models.SetAuthentication(Authentication).Delete(id);

            // results
            return statusCode == StatusCodes.Status404NotFound
                ? await this.ErrorResultAsync<string>($"Delete-RhinoModelCollection -Id {id} = NotFound", statusCode).ConfigureAwait(false)
                : NoContent();
        }

        // DELETE api/v3/models
        [HttpDelete]
        [SwaggerOperation(
            Summary = "Delete-RhinoModelCollection -All",
            Description = "Deletes all existing _**Rhino Model**_ collections.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(string))]
        public IActionResult Delete()
        {
            // delete
            _domain.Models.SetAuthentication(Authentication).Delete();

            // results
            return NoContent();
        }
        #endregion
    }
}
