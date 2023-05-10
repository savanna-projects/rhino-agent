/* CHANGE LOG - keep only last 5 threads
* 
* RESSOURCES
*/
using Gravity.Services.Comet.Engine.Attributes;

using Microsoft.CodeAnalysis;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Extensions;
using Rhino.Api.Interfaces;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Rhino.Controllers.Domain.Extensions
{
    /// <summary>
    /// Extension package for Rhino Agent Domain.
    /// </summary>
    internal static partial class RhinoExtensions
    {
        #region *** Expressions ***
        [GeneratedRegex("$", RegexOptions.Multiline)]
        private static partial Regex GetEndOfLineToken();

        [GeneratedRegex("(?<={).*(?=})")]
        private static partial Regex GetActionToken();
        #endregion

        // members
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// Gets a configuration from the data repository by the provided ID or a default configuration
        /// if the id provided is null or empty.
        /// </summary>
        /// <param name="repository">The IRepository instance.</param>
        /// <param name="id">The configuration ID.</param>
        /// <returns>A configuration from the data repository by the provided ID or a default configuration.</returns>
        public static RhinoConfiguration GetOrDefault(this IRepository<RhinoConfiguration> repository, string id)
        {
            // setup
            var driverParameters = new[]
            {
                new Dictionary<string, object>
                {
                    ["driver"] = "MockWebDriver",
                    ["driverBinaries"] = "."
                }
            };
            var connectorConfiguration = new RhinoConnectorConfiguration
            {
                Connector = "ConnectorText"
            };
            var defaultConfiguration = new RhinoConfiguration
            {
                DriverParameters = driverParameters,
                ConnectorConfiguration = connectorConfiguration
            };

            // get
            return string.IsNullOrEmpty(id) ? defaultConfiguration : repository.Get(id).Entity;
        }

        /// <summary>
        /// Parse RhinoTestCase specifications (one or more) into a collection of lines.
        /// </summary>
        /// <param name="rhinoTestCases">The RhinoTestCase(s) specifications.</param>
        /// <returns>A collection of lines.</returns>
        public static IEnumerable<IEnumerable<(int LineNumber, string Text)>> CreateLines(this string rhinoTestCases)
        {
            // setup
            var index = 0;
            var testCases = new List<IEnumerable<(int LineNumber, string Text)>>();
            var lines = GetEndOfLineToken().Split(input: rhinoTestCases)
                .Select(i => i.Trim())
                .ToList();

            // build
            var testResult = new List<(int LineNumber, string Text)>();
            foreach (var line in lines)
            {
                if (index != 0 && line.Trim().Equals(">>>"))
                {
                    testResult.Add((index++, line));
                    testCases.Add(testResult);
                    testResult = new List<(int LineNumber, string Text)>();
                    continue;
                }
                if(index != lines.Count)
                {
                    testResult.Add((index++, line));
                }
            }
            testCases.Add(testResult);

            // get
            return testCases;
        }

        /// <summary>
        /// Gets the TestRun key from the connector or an empty string if not possible.
        /// </summary>
        /// <param name="connector">The IConnector implementation.</param>
        /// <returns>The TestRun key.</returns>
        public static string GetRunKey(this IConnector connector)
        {
            return connector?.ProviderManager?.TestRun?.Key ?? string.Empty;
        }

        #region *** Symbols     ***
        /// <summary>
        /// Creates a collection of RhinoSymbolModel based on lines text and numbers (sent from or by IDE).
        /// </summary>
        /// <param name="testCase">The RhinoTestCase to create symbols for.</param>
        /// <param name="data">The symbols data (sent from or by IDE).</param>
        /// <returns>A collection of RhinoSymbolModel.</returns>
        public static RhinoSymbolModel CreateSymbols(this RhinoTestCase testCase, IEnumerable<(int LineNumber, string Text)> data)
        {
            // normalize
            testCase.ModelEntries ??= Array.Empty<RhinoPageModelEntry>();
            testCase.ModelEntries = testCase.ModelEntries.DistinctBy(i => i.Name, StringComparer.OrdinalIgnoreCase);

            // models
            var modelSymbols = testCase
                .ModelEntries
                .Select(i => CreateModelSymbol(i, data))
                .OrderBy(i => i.Name);

            // build
            var testSymbols = new RhinoSymbolModel
            {
                Name = testCase.Key,
                Details = testCase.Scenario,
                Type = "Module",
                Symbols = Array.Empty<RhinoSymbolModel>().Concat(modelSymbols)
            };
            var (lineNumber, text) = data.FirstOrDefault(i => i.Text.Contains(testCase.Key, Compare));
            var range = new RhinoSymbolRangeModel
            {
                Start = new RhinoSymbolPositionModel(0, lineNumber),
                End = new RhinoSymbolPositionModel(text.Length, lineNumber)
            };
            testSymbols.Range = range;
            testSymbols.SelectedRange = range;

            // build symbols
            var stepSymbols = new List<RhinoSymbolModel>();
            foreach (var testStep in testCase.Steps)
            {
                var symbols = CreateSymbols(testStep, data, 0);
                stepSymbols.Add(symbols);
            }

            // assign
            testSymbols.Symbols = testSymbols.Symbols.Concat(stepSymbols);

            // get
            return testSymbols;
        }

        private static RhinoSymbolModel CreateSymbols(RhinoTestStep testStep, IEnumerable<(int LineNumber, string Text)> data, int parent)
        {
            // conditions
            var isPlugin = testStep.Steps?.Any() == true;
            var isExpected = testStep.ExpectedResults?.Any() == true;

            // normalize
            SetStep(testStep);

            // build
            var (lineNumber, _) = GetLine(testStep, data, parent);
            var symbol = GetSymbolObjectModel(testStep, lineNumber);

            // exit conditions
            if (!isPlugin && !isExpected)
            {
                return symbol;
            }

            // expected results
            if (isExpected)
            {
                var assertActions = testStep.ExpectedResults.Select(i => new RhinoTestStep
                {
                    Action = i.ExpectedResult,
                    Command = "Assert"
                });
                var assertSymbols = assertActions.Select(i => CreateSymbols(testStep: i, data, lineNumber));
                symbol.Symbols = symbol.Symbols.Concat(assertSymbols);
            }

            // plugin (have nested actions)
            var symbols = new List<RhinoSymbolModel>();
            foreach (var step in testStep.Steps)
            {
                var nestedSymbol = CreateSymbols(step, data, lineNumber);
                symbols.Add(nestedSymbol);
            }
            symbol.Symbols = symbol.Symbols.Concat(symbols);

            // get
            return symbol;
        }

        private static void SetStep(RhinoTestStep testStep)
        {
            // conditions
            var isPlugin = testStep.Steps?.Any() == true;

            // normalize
            testStep.Steps ??= Array.Empty<RhinoTestStep>();
            testStep.Action = isPlugin
                ? GetActionToken().Match(testStep.Action).Value
                : testStep.Action;
            testStep.Command = string.IsNullOrEmpty(testStep.Command) ? "MissingPlugin" : testStep.Command;
        }

        private static (int LineNumber, string Text) GetLine(RhinoTestStep testStep, IEnumerable<(int LineNumber, string Text)> data, int parent)
        {
            // normalize
            var action = testStep.Action;
            foreach (var model in testStep.ModelEntries)
            {
                action = testStep.Action.Replace(model.Value, model.Name, Compare);
            }

            // setup
            var isAction = data.Any(i => i.Text.Contains(action, Compare));
            var lineNumber = isAction
                ? data.FirstOrDefault(i => i.Text.Contains(action, Compare)).LineNumber
                : parent;

            // get
            return data.FirstOrDefault(i => i.LineNumber == lineNumber);
        }

        private static RhinoSymbolModel GetSymbolObjectModel(RhinoTestStep testStep, int line)
        {
            // setup
            var symbol = new RhinoSymbolModel
            {
                Details = testStep.Action,
                Name = testStep.Command,
                Symbols = Array.Empty<RhinoSymbolModel>()
            };
            var range = new RhinoSymbolRangeModel
            {
                Start = new RhinoSymbolPositionModel(0, line),
                End = new RhinoSymbolPositionModel(symbol.Details.Length - 1, line)
            };
            symbol.Range = range;
            symbol.SelectedRange = range;
            symbol.Type = testStep.Command.ToUpper() switch
            {
                "ASSERT" => "Boolean",
                _ => testStep.Steps?.Any() == true ? "Class" : "Method",
            };

            // get
            return symbol;
        }

        private static RhinoSymbolModel CreateModelSymbol(RhinoPageModelEntry modelEntry, IEnumerable<(int LineNumber, string Text)> data)
        {
            // bad request
            if (modelEntry == default)
            {
                return default;
            }

            // setup
            var lines = data.Where(i => i.Text.Contains(modelEntry.Name, Compare));
            var symbol = new RhinoSymbolModel
            {
                Name = "Models",
                Details = "A collection of all models in use.",
                Line = lines.Min(i => i.LineNumber),
                Symbols = Array.Empty<RhinoSymbolModel>(),
                Type = "Module"
            };

            // build
            var symbols = new List<RhinoSymbolModel>();
            foreach (var (lineNumber, text) in lines)
            {
                var range = new RhinoSymbolRangeModel
                {
                    Start = new RhinoSymbolPositionModel(0, lineNumber),
                    End = new RhinoSymbolPositionModel(text.Length, lineNumber)
                };

                var start = text.IndexOf(modelEntry.Name, Compare);
                var end = start + modelEntry.Name.Length - 1;
                var selectedRange = new RhinoSymbolRangeModel
                {
                    Start = new RhinoSymbolPositionModel(start, lineNumber),
                    End = new RhinoSymbolPositionModel(end < 0 ? 0 : end, lineNumber)
                };

                var modelSymbol = new RhinoSymbolModel
                {
                    Name = modelEntry.Name,
                    Details = modelEntry.Value,
                    Range = range,
                    SelectedRange = selectedRange,
                    Type = "Model",
                    Line = lineNumber
                };

                symbols.Add(modelSymbol);
            }

            // set
            symbol.Symbols = symbol.Symbols.Concat(symbols);

            // get
            return symbol;
        }
        #endregion

        #region *** Cache         ***
        public static ConcurrentDictionary<string, ActionModel> GetActionsCache(this IDictionary<string, PluginCacheModel> models)
        {
            // setup
            var actions = new ConcurrentDictionary<string, ActionModel>();

            // iterate
            foreach (var model in models)
            {
                actions[model.Value.Plugin.Key] = model.Value.ActionModel;
            }

            // get
            return actions;
        }

        public static ConcurrentDictionary<string, ActionModel> GetActionsCache(this IEnumerable<PluginAttribute> attributes)
        {
            // setup
            var cache = new ConcurrentDictionary<string, ActionModel>(StringComparer.OrdinalIgnoreCase);

            // iterate
            foreach (var attribute in attributes)
            {
                var key = attribute.Name;
                var value = new ActionModel
                {
                    Entity = (ActionAttribute)attribute,
                    Key = attribute.Name,
                    Literal = attribute.Name.ToSpaceCase().ToLower(),
                    Source = ActionModel.ActionSource.Code,
                    Verb = "TBD"
                };

                cache[key] = value;
            }

            // get
            return cache;
        }

        public static ConcurrentDictionary<string, PluginCacheModel> GetPluginsCache(this IEnumerable<PluginAttribute> attributes)
        {
            // setup
            var cache = new ConcurrentDictionary<string, PluginCacheModel>(StringComparer.OrdinalIgnoreCase);

            // iterate
            foreach (var attribute in attributes)
            {
                var key = attribute.Name;
                cache[key] = GetPluginCacheModel(
                    source: ActionModel.ActionSource.Code,
                    path: default,
                    plugin: default,
                    (ActionAttribute)attribute);
            }

            // get
            return cache;
        }

        private static PluginCacheModel GetPluginCacheModel(string source, string path, RhinoPlugin plugin, ActionAttribute attribute)
        {
            // setup
            var actionModel = new ActionModel
            {
                Entity = attribute,
                Key = attribute.Name,
                Literal = attribute.Name.ToSpaceCase().ToLower(),
                Source = source,
                Verb = "TBD"
            };

            // get
            return new()
            {
                ActionModel = actionModel,
                Directory = Path.GetFileName(path),
                Path = Path.Exists(path) ? path : null,
                Plugin = plugin,
                Specifications = plugin == default ? null : plugin?.ToString()
            };
        }
        #endregion
    }
}
