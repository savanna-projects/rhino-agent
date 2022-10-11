/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// A range represents an ordered pair of two positions.
    /// </summary>
    [DataContract]
    public struct RhinoSymbolRangeModel
    {
        /// <summary>
        /// Gets or sets the end position. It is after or equal to `Start`.
        /// </summary>
        [DataMember]
        public RhinoSymbolPositionModel End { get; set; }

        /// <summary>
        /// Gets or sets the start position. It is before or equal to `End`.
        /// </summary>
        [DataMember]
        public RhinoSymbolPositionModel Start { get; set; }
    }
}
