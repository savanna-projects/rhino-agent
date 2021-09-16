/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 * 
 * WORK ITEMS
 * TODO: implement error handling
 * TODO: implement all CRUD actions
 */
using Gravity.Services.Comet;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Parser;
using Rhino.Api.Parser.Contracts;
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
    public class IntegrationController : ControllerBase
    {
        // members: constants
        private readonly string Seperator =
            Environment.NewLine + Environment.NewLine + Spec.Separator + Environment.NewLine + Environment.NewLine;
        private const string CountHeader = "Rhino-Total-Specs";

        // members: state
        private readonly IApplicationRepository applicationRepository;
        private readonly Orbit client;

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="applicationRepository">An IApplicationRepository implementation to use with the Controller.</param>
        /// <param name="client">An Orbit implementation to use with the Controller.</param>
        public IntegrationController(IApplicationRepository applicationRepository, Orbit client)
        {
            this.applicationRepository = applicationRepository;
            this.client = client;
        }

        [HttpPost("create")]
        [SwaggerOperation(
            Summary = "Create-TestCase",
            Description = "Creates a new _**Test Case**_ entity on the integrated application.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(IEnumerable<RhinoTestCase>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<RhinoIntegrationModel<string>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<RhinoIntegrationModel<string>>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<RhinoIntegrationModel<string>>))]
        public async Task<IActionResult> CreateTestCase([FromBody] RhinoIntegrationModel<TestCreateModel<string>> model)
        {
            // bad request
            if (model.Connector == null || model.Entity == null)
            {
                return await this
                    .ErrorResultAsync<string>("Create-TestCase = (BadRequest, NoConnector | Entity)")
                    .ConfigureAwait(false);
            }

            // parse test case & configuration
            var configuration = model.Connector;
            var spec = model.Entity.Spec.Split(Spec.Separator).Select(i => i.Trim()).ToArray();
            var testSuites = model.Entity.TestSuites;

            // text connector
            if (configuration.Connector.Equals(RhinoConnectors.Text))
            {
                Response.Headers.Add(CountHeader, $"{spec.Length}");
                return Created(string.Join(Seperator, spec), StatusCodes.Status200OK);
            }

            // convert into bridge object
            var testCases = new RhinoTestCaseFactory().GetTestCases(spec).ToArray();

            // build
            for (int i = 0; i < testCases.Length; i++)
            {
                testCases[i].TestSuites = testSuites;
                testCases[i].Priority = "2";
                testCases[i].Context["comment"] = Api.Extensions.Utilities.GetActionSignature("created");
            }
            applicationRepository.SetConnector(configuration);

            // create
            var responseBody = testCases.Select(i => applicationRepository.Add(i));

            // return results
            return Created("", responseBody);
        }
    }
}
