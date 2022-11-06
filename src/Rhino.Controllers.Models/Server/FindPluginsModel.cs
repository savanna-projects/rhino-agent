/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Contract for api/:version/meta action(s).
    /// </summary>
    [DataContract]
    public class FindPluginsModel
    {
        /// <summary>
        /// Gets or sets the filter expressions.
        /// </summary>
        [DataMember]
        public string Expression { get; set; }
    }
}
