/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Api.Contracts.AutomationProvider;

using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Contract for describing RhinoTestRun invocation results.
    /// </summary>
    [DataContract]
    public class GenericResultModel<T>
    {
        /// <summary>
        /// Gets or sets the TestRun status code.
        /// </summary>
        [DataMember]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the TestRun entity (after invocation).
        /// </summary>
        [DataMember]
        public T Entity { get; set; }

        /// <summary>
        /// Gets or sets the a message to return with the results.
        /// </summary>
        [DataMember]
        public string Message { get; set; }
    }
}
