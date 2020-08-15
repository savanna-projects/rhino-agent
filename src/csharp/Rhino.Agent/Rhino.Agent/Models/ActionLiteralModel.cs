using Gravity.Services.Comet.Engine.Attributes;

using System.Runtime.Serialization;

namespace Rhino.Agent.Models
{
    [DataContract]
    public class ActionLiteralModel
    {
        /// <summary>
        /// Gets or sets this action unique identifier
        /// </summary>
        [DataMember]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets this action literal expression (human format)
        /// </summary>
        [DataMember]
        public string Literal { get; set; }

        /// <summary>
        /// Gets or sets the verb used by this action to identify elementToActOn property
        /// </summary>
        /// <remarks>Verb must be supported by K.D.D engine</remarks>
        [DataMember]
        public string Verb { get; set; }

        /// <summary>
        /// Gets or sets the raw action KB of this action
        /// </summary>
        [DataMember]
        public ActionAttribute Action { get; set; }
    }
}
