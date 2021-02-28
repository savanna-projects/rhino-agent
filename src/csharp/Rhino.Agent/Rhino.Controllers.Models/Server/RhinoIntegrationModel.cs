/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Api.Contracts.Configuration;

using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Contract for api/:integration action(s).
    /// </summary>
    [DataContract]
    public class RhinoIntegrationModel<T>
    {
        /// <summary>
        /// Gets or sets the connector configuration by which to perform operation.
        /// </summary>
        [DataMember]
        public RhinoConnectorConfiguration Connector { get; set; }

        /// <summary>
        /// Gets or sets the entity on/by which to perform operation.
        /// </summary>
        [DataMember]
        public T Entity { get; set; }
    }
}
