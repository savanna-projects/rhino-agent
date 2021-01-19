using System;
using System.Runtime.Serialization;

namespace Rhino.Agent.Models
{
    /// <summary>
    /// Contract for describing RhinoTestCases NoSQL document.
    /// </summary>
    [DataContract]
    public class RhinoTestCaseDocument
    {
        /// <summary>
        /// Gets or sets a unique identifier (generated on run time by LiteDB engine).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the parent collection of this document.
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// Gets or sets the Rhino Spec.
        /// </summary>
        public string RhinoSpec { get; set; }
    }
}