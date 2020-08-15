using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Agent.Models
{
    /// <summary>
    /// Contract for describing RhinoTestCases NoSQL documents collection.
    /// </summary>
    [DataContract]
    public class RhinoTestCaseCollection
    {
        /// <summary>
        /// Gets or sets a unique identifier (generated on run time by LiteDB engine).
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a collection of serialized configurations which this collection can run with.
        /// </summary>
        [DataMember]
        public IList<string> Configurations { get; set; }

        /// <summary>
        /// Gets or sets a collection of RhinoTestCases NoSQL document item.
        /// </summary>
        [DataMember]
        public IList<RhinoTestCaseDocument> RhinoTestCaseDocuments { get; set; }
    }
}