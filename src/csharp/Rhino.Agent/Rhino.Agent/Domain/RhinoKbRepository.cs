/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;
using Gravity.Services.Comet;
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

using Microsoft.Extensions.DependencyInjection;

using Rhino.Agent.Extensions;
using Rhino.Agent.Models;
using Rhino.Api.Parser;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhino.Agent.Domain
{
    public class RhinoKbRepository : Repository
    {
        // members: state
        private readonly RhinoPluginRepository rhinoPlugin;
        private static IDictionary<string, string[]> VerbMap => new Dictionary<string, string[]>
        {
            ["into"] = new[] { ActionType.SendKeys, ActionType.TrySendKeys },
            ["take"] = new[] { ActionType.SelectFromComboBox, ActionType.RegisterParameter, ActionType.GoToUrl },
            ["of"] = new[] { ActionType.GetScreenshot }
        };

        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Domain.RhinoTestCaseRepository.</param>
        public RhinoKbRepository(IServiceProvider provider)
            : base(provider)
        {
            Client = provider.GetRequiredService<Orbit>();
            Macros = Client.Macros().Select(i => Client.Macros(i));
            Operators = new RhinoTestCaseFactory(Client).OperatorsMap;
            rhinoPlugin = provider.GetRequiredService<RhinoPluginRepository>();
        }

        #region *** properties   ***
        /// <summary>
        /// Gets the orbit client used in this instance
        /// </summary>
        public Orbit Client { get; }

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
        /// Gets a list of all available actions including Rhino Plugins.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get logs.</param>
        /// <returns>A collection of ActionAttribute.</returns>
        public IEnumerable<(string Type, ActionAttribute Model)> GetActionsAsAttributes(Authentication authentication)
        {
            return DoGetActions(authentication);
        }

        /// <summary>
        /// Gets a list of all available actions including Rhino Plugins.
        /// </summary>
        /// <param name="authentication">Authentication object by which to get logs.</param>
        /// <returns>A collection of action names (PascalCase).</returns>
        public IEnumerable<string> GetActions(Authentication authentication)
        {
            // setup
            var pluginSpecs = rhinoPlugin.Get(authentication).data;
            var pluginObjcs = new RhinoPluginFactory().GetRhinoPlugins(pluginSpecs.ToArray());

            // return all actions
            return Client.Actions().Concat(pluginObjcs.Select(i => i.Key));
        }

        /// <summary>
        /// Gets a list of non conditional actions with their literals default verbs
        /// </summary>
        /// <returns>List of non conditional actions</returns>
        public IEnumerable<(string Type, ActionLiteralModel Model)> GetActionsLiteral(Authentication authentication)
        {
            var actions = new List<(string Type, ActionLiteralModel Model)>();
            foreach (var onAction in DoGetActions(authentication))
            {
                try
                {
                    var model = new ActionLiteralModel
                    {
                        Key = onAction.Action.Name,
                        Literal = onAction.Action.Name.PascalToSpaceCase(),
                        Verb = GetVerb(onAction.Action.Name),
                        Action = onAction.Action
                    };
                    actions.Add((onAction.Type, model));
                }
                catch (Exception e) when (e != null)
                {
                    // ignore exceptions
                }
            }
            return actions;
        }

        // gets a verb for this action from default verbs map
        private static string GetVerb(string action)
        {
            var verb = VerbMap.FirstOrDefault(i => i.Value.Contains(action)).Key;
            return verb == default ? "on" : verb;
        }

        private IEnumerable<(string Type, ActionAttribute Action)> DoGetActions(Authentication authentication)
        {
            // setup
            var pluginSpecs = rhinoPlugin.Get(authentication).data;
            var pluginObjcs = new RhinoPluginFactory().GetRhinoPlugins(pluginSpecs.ToArray());

            // convert
            var attributes = pluginObjcs.Select(i => ("plugin", i.ToActionAttribute()));

            // return all actions
            return Client.Actions().Select(i => ("code", Client.Actions(i))).Concat(attributes);
        }
    }
}