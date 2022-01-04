/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for describing RhinoEnvironmentModel NoSQL document.
    /// </summary>
    [DataContract]
    public class RhinoEnvironmentModel
    {
        /// <summary>
        /// Gets or sets a unique identifier (generated on runtime by LiteDB engine).
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a unique name of this environment.
        /// </summary>
        [DataMember]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Environment data to sync with Gravity static in-memory environment.
        /// </summary>
        [DataMember]
        public IDictionary<string, object> Environment { get; set; } = new ConcurrentDictionary<string, object>();
    }
}