/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Response contract for api/:version/configurations controller.
    /// </summary>
    [DataContract]
    public class ConfigurationResponseModel
    {
        /// <summary>
        /// Gets or sets the id of the entity in the domain state.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the PageModelCollections used by the configuration.
        /// </summary>
        [DataMember]
        public IEnumerable<string> Elements { get; set; }

        /// <summary>
        /// Gets or sets the TestCaseCollections used by the configuration.
        /// </summary>
        [DataMember]
        public IEnumerable<string> Tests { get; set; }
    }
}