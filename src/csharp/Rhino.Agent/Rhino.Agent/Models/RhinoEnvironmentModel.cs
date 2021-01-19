using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Agent.Models
{
    /// <summary>
    /// Contract for describing RhinoTestCases NoSQL documents collection.
    /// </summary>
    [DataContract]
    public class RhinoEnvironmentModel
    {
        /// <summary>
        /// Gets or sets a unique identifier (generated on run time by LiteDB engine).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a unique name of this environment.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Environment data to sync with Gravity static in-memory environment.
        /// </summary>
        public IDictionary<string, object> Environment { get; set; } = new ConcurrentDictionary<string, object>();
    }
}
