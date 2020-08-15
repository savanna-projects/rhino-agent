using Rhino.Api.Contracts.AutomationProvider;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Agent.Models
{
    /// <summary>
    /// Contract for describing RhinoTestCases NoSQL documents collection.
    /// </summary>
    [DataContract]
    public class RhinoPageModelCollection
    {
        /// <summary>
        /// Gets or sets a unique identifier (generated on run time by LiteDB engine).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a collection of serialized configurations which this collection can run with.
        /// </summary>
        public IList<string> Configurations { get; set; }

        /// <summary>
        /// A collection of Rhino.Api.Contracts.AutomationProvider.RhinoPageModel
        /// </summary>
        public IList<RhinoPageModel> Models { get; set; }
    }
}