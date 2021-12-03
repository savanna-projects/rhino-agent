/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for describing RhinoTestCases NoSQL documents collection.
    /// </summary>
    [DataContract]
    public class RhinoTestCollection
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
        public IList<string> Configurations { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a collection of RhinoTestCases NoSQL document item.
        /// </summary>
        [DataMember]
        public IList<RhinoTestModel> RhinoTestCaseModels { get; set; } = new List<RhinoTestModel>();
    }
}