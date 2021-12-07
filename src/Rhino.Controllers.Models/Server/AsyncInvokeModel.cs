/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for api/:version/async/rhino controller.
    /// </summary>
    [DataContract]
    public class AsyncInvokeModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the invoke.
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the status code of the invoke request.
        /// </summary>
        [DataMember]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the endpoint from which you can fetch the invoke status.
        /// </summary>
        [DataMember]
        public string StatusEndpoint { get; set; }

        /// <summary>
        /// Gets or sets additional context information.
        /// </summary>
        [DataMember]
        public object Context { get; set; }
    }
}