/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for describing RhinoTestCases NoSQL document.
    /// </summary>
    [DataContract]
    public class RhinoTestModel
    {
        /// <summary>
        /// Gets or sets a unique identifier (generated on run time by LiteDB engine).
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the parent collection of this document.
        /// </summary>
        [DataMember]
        public string Collection { get; set; }

        /// <summary>
        /// Gets or sets the Rhino Spec.
        /// </summary>
        [DataMember]
        public string RhinoSpec { get; set; }
    }
}