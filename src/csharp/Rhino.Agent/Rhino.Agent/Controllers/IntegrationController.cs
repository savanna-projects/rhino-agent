/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Castle.Core.Configuration;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Rhino.Agent.Domain;
using Rhino.Api.Parser.Contracts;

using System;
using System.Collections.Generic;

namespace Rhino.Agent.Controllers
{
    [Route("api/v3/[controller]")]
    [Route("api/latest/[controller]")]
    public class IntegrationController : Controller
    {
        // constants
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;
        private readonly string Seperator =
            Environment.NewLine + Environment.NewLine + SpecSection.Separator + Environment.NewLine + Environment.NewLine;

        // members: state
        private readonly IServiceProvider provider;
        private readonly IEnumerable<Type> types;
        private readonly IConfiguration appSettings;
        private readonly RhinoConfigurationRepository configurationRepository;
        private readonly RhinoModelRepository modelRepository;
        private readonly RhinoTestCaseRepository testCaseRepository;

        /// <summary>
        /// Creates a new instance of Rhino.Agent.Controllers.RhinoController.
        /// </summary>
        /// <param name="provider">Services container.</param>
        public IntegrationController(IServiceProvider provider)
        {
            // provider
            this.provider = provider;

            // state
            types = provider.GetRequiredService<IEnumerable<Type>>();
            appSettings = provider.GetRequiredService<IConfiguration>();
            configurationRepository = provider.GetRequiredService<RhinoConfigurationRepository>();
            modelRepository = provider.GetRequiredService<RhinoModelRepository>();
            testCaseRepository = provider.GetRequiredService<RhinoTestCaseRepository>();
        }

        [HttpPost("tests/execute")]
        public IActionResult Execute()
        {
            throw new NotImplementedException();
        }

        [HttpPost("tests/create")]
        public IActionResult Create()
        {
            throw new NotImplementedException();
        }

        [HttpPut("tests/{id}")]
        public IActionResult UpdateTest(string id)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("tests/{id}")]
        public IActionResult DeleteTest(string id)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("runs/{id}")]
        public IActionResult DeleteRun(string id)
        {
            throw new NotImplementedException();
        }
    }
}