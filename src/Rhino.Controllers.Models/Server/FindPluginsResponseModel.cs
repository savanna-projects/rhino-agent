/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Contract for api/:version/meta action(s).
    /// </summary>
    [DataContract]
    public class FindPluginsResponseModel
    {
        /// <summary>
        /// Gets or sets the plugin name.
        /// </summary>
        [DataMember, JsonPropertyName("Key")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the plugin summary.
        /// </summary>
        [DataMember]
        public string Summary { get; set; }
    }
}
