using Gravity.Abstraction.Logging;
using Gravity.Extensions;
using Gravity.Services.Comet;
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

using Newtonsoft.Json;

using Rhino.Agent.Models;
using Rhino.Api.Extensions;
using Rhino.Api.Parser;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Rhino.Agent.Components
{
    public class KnowledgeBaseManager
    {
        // constants: logging
        private const string E1 = "Cannot find [{0}] resource under [{1}] assembly manifest.";
        private const string M2 = "Knowledge Base resource [{0}] created.";

        // constants
        private const string Root = "knowledge_base";
        private const string ActionsFolder = Root + @"\actions";
        private const string MacrosFolder = Root + @"\macros";
        private const string Locators = Root + @"\available_locators.txt";
        private const string ActionsList = Root + @"\available_actions.txt";
        private const string MacrosList = Root + @"\available_macros.txt";
        private const string OperatorsList = Root + @"\available_operators.txt";
        private const string ReadMe = "README.md";
        private const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

        private static IDictionary<string, string[]> VerbMap => new Dictionary<string, string[]>
        {
            ["into"] = new[] { ActionType.SendKeys, ActionType.TrySendKeys },
            ["take"] = new[] { ActionType.SelectFromComboBox, ActionType.RegisterParameter, ActionType.GoToUrl },
            ["of"] = new[] { ActionType.GetScreenshot }
        };

        #region *** constructors ***
        /// <summary>
        /// creates new knowledge base manager instance
        /// </summary>
        public KnowledgeBaseManager()
            : this(new Orbit(Api.Extensions.Utilities.Types)) { }

        /// <summary>
        /// creates new knowledge base manager instance
        /// </summary>
        /// <param name="client">orbit client from which to fetch knowledge base information</param>
        public KnowledgeBaseManager(Orbit client)
            : this(client, Api.Extensions.Utilities.CreateDefaultLogger()) { }

        /// <summary>
        /// creates new knowledge base manager instance
        /// </summary>
        /// <param name="client">orbit client from which to fetch knowledge base information</param>
        /// <param name="logger">logger implementation to use for this knowledge base manager</param>
        public KnowledgeBaseManager(Orbit client, ILogger logger)
        {
            Logger = logger.Setup(nameof(KnowledgeBaseManager));
            Client = client;
            Actions = client.Actions().Select(i => client.Actions(i));
            Macros = Client.Macros().Select(i => client.Macros(i));
            Operators = new RhinoTestCaseFactory(Client).OperatorsMap;
        }
        #endregion

        #region *** properties   ***
        /// <summary>
        /// gets the logger of the current application context
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the orbit client used in this instance
        /// </summary>
        public Orbit Client { get; }

        /// <summary>
        /// Gets a complete actions knowledge base, from all available actions in this domain
        /// </summary>
        public IEnumerable<ActionAttribute> Actions { get; }

        /// <summary>
        /// Gets a complete macros knowledge base, from all available macros in this domain
        /// </summary>
        public IEnumerable<MacroAttribute> Macros { get; }

        /// <summary>
        /// Gets all available operators under this domain
        /// </summary>
        public IDictionary<string, string> Operators { get; }
        #endregion

        /// <summary>
        /// Generates a complete actions/macros knowledge base files under the current folder.
        /// </summary>
        /// <param name="path">Path under which to create the knowledge base files.</param>
        public void GenerateKnowledgeBase(string path)
        {
            // shortcuts
            var s = Assembly.GetExecutingAssembly();
            var m = Client.Macros();
            var a = Client.Actions();
            var l = Client.Locators().Select(i => i.PascalToSpaceCase());
            var o = new RhinoTestCaseFactory(Client).OperatorsMap.Select(i => i.Value);

            // layout
            path = path.EndsWith("\\") ? path : $"{path}\\";
            Directory.CreateDirectory(path + ActionsFolder);
            Directory.CreateDirectory(path + MacrosFolder);

            // generate knowledge base-files
            GenerateActionFiles(a, path);
            GenerateMacroFiles(m, path);
            GenerateReadmeFile(s, path);
            File.WriteAllLines($"{path}\\{Locators}", l);
            File.WriteAllLines($"{path}\\{OperatorsList}", o);
        }

        // generate main help file (README.md)
        private void GenerateReadmeFile(Assembly a, string path)
        {
            // fetch readme.md file from assembly manifest
            var resourceName = Array.Find(a.GetManifestResourceNames(), str => str.EndsWith(ReadMe, Compare));

            // exit conditions
            if (string.IsNullOrEmpty(resourceName))
            {
                Logger?.ErrorFormat(E1, ReadMe, a.FullName);
                return;
            }

            // shortcuts
            var f = $"{path}\\{Root}\\{ReadMe}";

            // write file
            using (var reader = new StreamReader(a.GetManifestResourceStream(resourceName)))
            {
                var syntax = reader.ReadToEnd();
                File.WriteAllText(f, syntax);
            }
            Logger?.DebugFormat(M2, f);
        }

        // generate individual action-files
        private void GenerateActionFiles(IEnumerable<string> actions, string path)
        {
            // write available-actions.txt file
            var actionsList = actions.Select(i => i.PascalToSpaceCase());
            File.WriteAllLines($"{path}\\{ActionsList}", actionsList);

            // generate individual action file
            var directory = $"{path}\\{ActionsFolder}\\";
            Directory.CreateDirectory(directory);

            actions.AsParallel().ForAll(a =>
            {
                var name = a.PascalToKebabCase();
                var json = JsonConvert.SerializeObject(Client.Actions(a), Formatting.Indented);
                var fileName = $"{directory}{name}.json";
                File.WriteAllText(fileName, json);
                Logger?.DebugFormat(M2, fileName);
            });
        }

        // generate individual macro-files
        private void GenerateMacroFiles(IEnumerable<string> macros, string path)
        {
            // write available-macros.txt file
            var macrosList = macros.Select(i => i.PascalToSpaceCase());
            File.WriteAllLines($"{path}\\{MacrosList}", macrosList);

            // generate individual macro file
            var directory = $"{path}\\{MacrosFolder}\\";
            Directory.CreateDirectory(directory);

            macros.AsParallel().ForAll(a =>
            {
                var name = a.PascalToKebabCase();
                var json = JsonConvert.SerializeObject(Client.Macros(a), Formatting.Indented);
                var fileName = $"{directory}{name}.json";
                File.WriteAllText(fileName, json);
                Logger?.DebugFormat(M2, fileName);
            });
        }

        /// <summary>
        /// Gets a list of non conditional actions with their literals default verbs
        /// </summary>
        /// <returns>List of non conditional actions</returns>
        public IEnumerable<ActionLiteralModel> GetActionsLiteral()
        {
            var actions = new List<ActionLiteralModel>();
            foreach (var onAction in Actions)
            {
                try
                {
                    var model = new ActionLiteralModel
                    {
                        Key = onAction.Name,
                        Literal = onAction.Name.PascalToSpaceCase(),
                        Verb = GetVerb(onAction.Name),
                        Action = onAction
                    };
                    actions.Add(model);
                }
                catch (Exception e) when (e != null)
                {
                    // ignore exceptions
                }
            }
            return actions;
        }

        // gets a verb for this action from default verbs map
        private string GetVerb(string action)
        {
            var verb = VerbMap.FirstOrDefault(i => i.Value.Contains(action)).Key;
            return verb == default ? "on" : verb;
        }
    }
}
