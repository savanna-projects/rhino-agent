/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for async status results.
    /// </summary>
    [DataContract]
    public class AsyncStatusModel<T>
    {
        /// <summary>
        /// Gets or sets the status of the invoke.
        /// </summary>
        [DataMember]
        public string Status { get; set; }

        /// <summary>
        /// The unique identifier of the status model.
        /// </summary>
        [DataMember]
        public string RuntimeId { get; set; }

        /// <summary>
        /// Gets or sets the status code of the invoke.
        /// </summary>
        [DataMember]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the progress of the invoke (currently not supported).
        /// </summary>
        [DataMember]
        public double Progress { get; set; }

        /// <summary>
        /// Gets or sets the entity to return when invoke completes.
        /// </summary>
        [DataMember]
        public object EntityOut { get; set; }

        /// <summary>
        /// Gets or sets the invoked entity.
        /// </summary>
        [DataMember]
        public T EntityIn { get; set; }
    }
}