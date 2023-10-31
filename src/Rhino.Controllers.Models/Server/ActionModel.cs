/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet.Engine.Attributes;

using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for api/:version/meta action(s).
    /// </summary>
    [DataContract]
    public class ActionModel : BaseModel<ActionAttribute>
    {
        /// <summary>
        /// List of available sources for ActionModel.
        /// </summary>
        public static class ActionSource
        {
            public const string Code = "code";
            public const string External = "external";
            public const string Plugin = "plugin";
        }

        /// <summary>
        /// Gets or sets the source of the plugin (e.g. code, plugin, etc.).
        /// </summary>
        [DataMember]
        public string Source { get; set; } = ActionSource.Code;

        // TODO: implement object to MD to allow dynamic documentation generator and remove documentation redundancy.
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
