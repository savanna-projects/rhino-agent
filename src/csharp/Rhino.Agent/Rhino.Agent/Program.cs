using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Gravity.Abstraction.Logging;
using Gravity.Services.Comet.Engine.Core;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Newtonsoft.Json.Linq;

using Rhino.Agent.Components;
using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Extensions;
using Rhino.Api.Contracts.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rhino.Agent
{
    public static class Program
    {
        // constants
        private const string Configuration = "configuration";
        private const string KB = "kb";
        private const string Generate = "generate";
        private const string License = "license";
        private const string Delete = "delete";
        private const string Connect = "connect";

        // members: state
        private static IDictionary<string, string> arguments;
        private static int errorCode;
        private static ILogger logger;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                CreateWebHostBuilder(args).Build().Run();
                return;
            }

            // parse CLI arguments & apply license if available
            var cli = "{{$ " + string.Join(" ", args) + "}}";
            arguments = new CliFactory(cli).Parse();
            ApplyLicense();

            // get all available types
            var types = Utilities.Types;

            // setup            
            var configuration = SetConfiguration();
            logger = Utilities.CreateDefaultLogger(configuration).CreateChildLogger(nameof(Program));
            logger.Info("Configuration setup completed.");

            var connector = SetConnector(types, configuration);
            errorCode = configuration.EngineConfiguration.ErrorOnExitCode;

            // pipeline
            DeleteRuns(connector);
            GenerateKb(types);
            ExportTestCases(connector);
            var outcome = connector.Connect().Execute();
            ProcessOutcome(outcome);
        }

        #region *** pipeline: server  ***
        // creates web service host container
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>();
        #endregion

        #region *** pipeline: setup   ***
        private static RhinoConfiguration SetConfiguration()
        {
            OnArgumentsError(Configuration);

            // generate configuration token
            var json = File.ReadAllText(arguments[Configuration]);
            var token = JToken.Parse(json);

            // generate configuration
            var configuration = token.ToObject<RhinoConfiguration>();

            // finalize
            configuration.Name = Path.GetFileNameWithoutExtension(arguments[Configuration]);
            return configuration;
        }

        private static IConnector SetConnector(IEnumerable<Type> types, RhinoConfiguration configuration)
        {
            // constants
            const StringComparison C = StringComparison.OrdinalIgnoreCase;

            // types loading pipeline
            var byContract = types.Where(t => typeof(IConnector).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
            var byAttribute = byContract.Where(t => t.GetCustomAttribute<ConnectorAttribute>() != null);

            // exit conditions - will abort on azure devOps task as well
            OnConnectorError(configuration, !byAttribute.Any());

            // get connector type by it's name
            var type = byAttribute
                .FirstOrDefault(t => t.GetCustomAttribute<ConnectorAttribute>().Value.Equals(configuration.ConnectorConfiguration.Connector, C));

            // exit conditions - will abort on azure devops task as well
            OnConnectorError(configuration, type == default);

            // activate new connector instance
            var connector = (IConnector)Activator.CreateInstance(type, new object[] { configuration, types });

            // exit conditions
            if (connector != default && arguments.ContainsKey(Connect))
            {
                Console.WriteLine("successfully connected");
                Environment.Exit(0);
            }
            return connector;
        }
        #endregion

        #region *** pipeline: process ***
        private static void GenerateKb(IEnumerable<Type> types)
        {
            // constants: logging
            const string M = "knowledge base files, created under [{0}]";

            // exit conditions
            if (!arguments.ContainsKey(KB))
            {
                return;
            }

            // generate KB directory
            var exists = Directory.Exists($"{arguments[KB]}");
            var path = exists ? $"{arguments[KB]}" : Environment.CurrentDirectory;

            // generate KB files
            new KnowledgeBaseManager(new Gravity.Services.Comet.Orbit(types)).GenerateKnowledgeBase(path);
            var message = string.Format(M, arguments[KB]);
            Console.WriteLine(message);

            // exit application with success code
            Environment.Exit(0);
        }

        private static void ExportTestCases(IConnector connector)
        {
            // constants: logging
            const string M = "gravity requests were successfully exported to [{0}]";

            // exit conditions
            if (!arguments.ContainsKey(Generate))
            {
                return;
            }

            // create output folder
            var exists = Directory.Exists($"{arguments[Generate]}");
            var path = exists ? $"{arguments[Generate]}" : $"{Environment.CurrentDirectory}\\api_requests";

            // export
            connector.ProviderManager.ExportTestCases(path);
            Console.WriteLine(string.Format(M, path));

            // exit application with success code
            Environment.Exit(0);
        }

        private static void ProcessOutcome(RhinoTestRun outcome)
        {
            // output
            var jsonSettings = Gravity.Extensions.Utilities.GetJsonSettings<CamelCaseNamingStrategy>(Formatting.Indented);
            Console.WriteLine(JsonConvert.SerializeObject(outcome, jsonSettings));

            // constants: logging
            const string M =
                "Total of [{0}] tests failed and stopped the process. Please review your tests results report.";

            // exit conditions
            if (outcome.TotalFail == 0)
            {
                Environment.Exit(0);
            }

            // exit with failure code
            Console.Error.WriteLine(string.Format(M, outcome.TotalFail));
            Environment.Exit(errorCode);
        }

        private static void ApplyLicense()
        {
            // constants: logging
            const string M = "License [license.lcn] successfully updated.";

            // exit conditions
            if (!arguments.ContainsKey(License))
            {
                return;
            }

            // apply license
            File.WriteAllText("license.lcn", arguments[License]);
            Console.WriteLine(M);
            Environment.Exit(0);
        }

        private static void DeleteRuns(IConnector connector)
        {
            // exit conditions
            if (!arguments.ContainsKey(Delete))
            {
                return;
            }

            // delete test-runs
            var keys = arguments[Delete].Trim().Split(' ');
            connector.ProviderManager.DeleteTestRun(keys);
            Environment.Exit(0);
        }
        #endregion

        #region *** pipeline: error   ***
        private static void OnArgumentsError(string argument)
        {
            // constants: logging
            const string M = "was not able to find a [{0}] argument, please make sure to pass a arguments list";

            // exit conditions
            if (HasArgumnet(argument))
            {
                return;
            }

            // exit application
            Console.Error.WriteLine(string.Format(M, argument));
            Environment.Exit(errorCode);
        }

        private static void OnConnectorError(RhinoConfiguration configuration, bool isError)
        {
            // constants: logging
            const string M = "Connector [{0}] was not found. Please make sure to pass a valid connector.";

            // exit conditions
            if (!isError)
            {
                return;
            }

            // failure on getting connector
            Console.Error.WriteLine(string.Format(M, configuration.ConnectorConfiguration.Connector));
            Environment.Exit(errorCode);
        }

        private static bool HasArgumnet(string key)
            => arguments.ContainsKey(key) && !string.IsNullOrEmpty(arguments[key]);
        #endregion
    }
}