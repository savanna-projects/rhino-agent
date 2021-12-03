using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.Comet.Engine.Plugins;
using Gravity.Services.DataContracts;

using OpenQA.Selenium;

namespace Rhino.Plugins.Custom
{
    [Macro("SampleMacro.json", Name = "smpl")]
    public class MacroPluginSample : MacroPlugin
    {
        // Will use the assemblies loaded by into Gravity domain (will not initiate load)
        public MacroPluginSample(IWebDriver webDriver, AutomationEnvironment environment)
            : base(webDriver, environment)
        { }

        public override string OnPerform(string cli)
        {
            return "Hello from Macro plugin";
        }
    }
}
