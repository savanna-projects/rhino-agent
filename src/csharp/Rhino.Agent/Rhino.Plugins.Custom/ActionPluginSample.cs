using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.Comet.Engine.Plugins;
using Gravity.Services.DataContracts;

using OpenQA.Selenium;

using System;
using System.Collections.Generic;
namespace Rhino.Plugins.Custom
{
    [Action("SampleAction.json", Name = "SampleAction")]
    public class ActionPluginSample : ActionPlugin
    {
        // Will use the types injected by gravity clients
        public ActionPluginSample(IWebDriver webDriver, WebAutomation webAutomation, IEnumerable<Type> types)
            : base(webDriver, webAutomation, types)
        { }

        public override void OnPerform(ActionRule actionRule)
        {
            Console.WriteLine("Hello from action plugin");
        }
    }
}
