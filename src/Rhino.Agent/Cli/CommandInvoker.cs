/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Cli;
using Gravity.Services.Comet.Engine.Core;

using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Interfaces;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace Rhino.Agent.Cli
{
    public sealed class CommandInvoker
    {
        // members: static
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // members
        private readonly IDictionary<string, string> _arguments;
        private readonly IEnumerable<Type> _types;

        /// <summary>
        /// Creates a new CommandInvoker instance.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public CommandInvoker(IEnumerable<Type> types, string[] args)
        {
            _arguments = new CliFactory("{{$ " + string.Join(" ", args) + "}}").Parse();
            _types = types;
        }

        public void Invoke()
        {
            // constants
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

            // setup
            var methods = GetType()
                .GetMethods(Flags)
                .Where(i => i.GetCustomAttribute<CommandAttribute>() != null)
                .OrderBy(i => i.GetCustomAttribute<CommandAttribute>()?.Order);

            // invoke
            foreach (var method in methods)
            {
                var command = method.GetCustomAttribute<CommandAttribute>()?.Command;
                command = string.IsNullOrEmpty(command) ? string.Empty : command;

                if (!_arguments.Keys.Select(i => i.ToUpper()).Contains(command.ToUpper()))
                {
                    continue;
                }

                var instance = method.IsStatic ? null : this;
                method.Invoke(instance, new object[] { _arguments });
            }
        }

        [Command(command: "license", order: 1)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by reflection")]
        private void GetLicenseCommand(IDictionary<string, string> arguments)
        {
            // constants
            const string License = "license";

            // assert
            AssertArgument(License);

            // apply license
            File.WriteAllText("license.lcn", arguments[License]);
            Console.WriteLine($"Set-License -License {arguments[License]} = OK");
        }

        [Command(command: "delete", order: 2)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by reflection")]
        private void GetDeleteRunsCommand(IDictionary<string, string> arguments)
        {
            // constants
            const string Configuration = "configuration";
            const string Delete = "delete";

            // assert
            AssertArgument(Configuration);
            AssertArgument(Delete);

            // setup
            var configuration = GetConfiguration(arguments[Configuration]);
            var connector = GetConnector(_types, configuration);
            var keys = arguments[Delete].Trim().Split(' ');

            // invoke
            connector?.ProviderManager.DeleteTestRun(keys);
        }

        [Command(command: "generate", order: 3)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by reflection")]
        private void GetGenerateCommand(IDictionary<string, string> arguments)
        {
            // constants
            const string Configuration = "configuration";
            const string Generate = "generate";

            // assert
            AssertArgument(Configuration);

            // create output folder
            var isPath = !string.IsNullOrEmpty(arguments[Generate]);
            var path = isPath
                ? $"{arguments[Generate]}"
                : Path.Combine($"{Environment.CurrentDirectory}", "RhinoExportResults");
            Directory.CreateDirectory(path);

            // setup
            var configuration = GetConfiguration(arguments[Configuration]);
            var connector = GetConnector(_types, configuration);

            // export
            connector?.ProviderManager.ExportTestCases(path);

            // log
            Console.WriteLine($"Export-RhinoToGravity -Path {path} = OK");
        }

        [Command(command: "configuration", order: 4)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by reflection")]
        private void GetConfigurationCommand(IDictionary<string, string> arguments)
        {
            // constants
            const string Configuration = "configuration";

            // assert
            AssertArgument(Configuration);

            // setup
            var configuration = GetConfiguration(arguments[Configuration]);
            var connector = GetConnector(_types, configuration);

            // invoke
            var testRun = connector?.Connect().Invoke();

            // log
            if(testRun != null)
            {
                Console.WriteLine(JsonSerializer.Serialize(testRun, s_jsonOptions));
            }

            // OK
            if (testRun?.TotalFail == 0)
            {
                Environment.Exit(0);
            }

            // some tests failed
            var code = configuration?.EngineConfiguration.ErrorOnExitCode == null
                ? 10
                : configuration.EngineConfiguration.ErrorOnExitCode;
            Console.Error.WriteLine($"Invoke-Tests -Fail {testRun?.TotalFail} -Pass {testRun?.TotalPass} = TestsFailed");
            Environment.Exit(code);
        }

        [Command(command: "connect", order: 5)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by reflection")]
        private void GetConnectCommand(IDictionary<string, string> arguments)
        {
            // constants
            const string Configuration = "configuration";

            // assert
            AssertArgument(Configuration);

            // setup
            var configuration = GetConfiguration(arguments[Configuration]);
            var connector = GetConnector(_types, configuration);

            // invoke
            connector?.Connect();
        }

        // Utilities
        private void AssertArgument(string key)
        {
            // setup
            var hasArgument = _arguments.ContainsKey(key) && !string.IsNullOrEmpty(_arguments[key]);

            // exit conditions
            if (hasArgument)
            {
                return;
            }

            // exit application
            Console.Error.WriteLine($"Get-Argument -Argument {key} = (NotFound | NoValue)");
            Environment.Exit(0);
        }

        private static RhinoConfiguration GetConfiguration(string path)
        {
            // generate configuration
            var json = File.ReadAllText(path);
            var configuration = JsonSerializer.Deserialize<RhinoConfiguration>(json, s_jsonOptions);

            // bad request
            if(configuration == null)
            {
                Console.Error.WriteLine("Get-Configuration " +
                    $"-Path {path} = (BadRequst | NotConfiguration | InvalidJsonFormat)");
                return default;
            }

            // setup
            configuration.Name = Path.GetFileNameWithoutExtension(path);

            // get
            return configuration;
        }

        private static IConnector GetConnector(
            IEnumerable<Type> types, RhinoConfiguration configuration)
        {
            // constants
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // types loading pipeline
            var connectorType = configuration?.ConnectorConfiguration.Connector;
            var connectorTypes = types.Where(t => typeof(IConnector).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
            var connectors = connectorTypes.Where(t => t.GetCustomAttribute<ConnectorAttribute>() != null);
            var errorOnExitCode = (configuration?.EngineConfiguration.ErrorOnExitCode) ?? 0;

            // exit conditions - will abort on azure devOps task as well
            if (!connectors.Any())
            {
                Console.Error
                    .WriteLine("Get-Connector -Type all = NotFound");
                Environment.Exit(errorOnExitCode);
            }

            // get connector type by it's name
            var type = connectors
                .FirstOrDefault(t => t.GetCustomAttribute<ConnectorAttribute>()?.Value.Equals(connectorType, Compare) == true);

            // exit conditions - will abort on azure devOps task as well
            if (type == default)
            {
                Console.Error
                    .WriteLine($"Get-Connector -Type {connectorType} = NotFound");
                Environment.Exit(errorOnExitCode);
            }

            // activate new connector instance
            // exit conditions
            if (Activator.CreateInstance(type, new object[] { configuration, types }) is not IConnector connector)
            {
                return default;
            }

            // get
            Console.WriteLine($"Get-Connector -Type {connectorType} = OK");
            return connector;
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class CommandAttribute : Attribute
        {
            public CommandAttribute(string command, double order)
            {
                Command = command;
                Order = order;
            }

            public string Command { get; set; }
            public double Order { get; set; }
        }
    }
}
