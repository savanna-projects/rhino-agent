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
    public class ModelCollectionResponseModel
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
        public IEnumerable<string> Configurations { get; set; }

        /// <summary>
        /// Gets or sets the total models under the models collection.
        /// </summary>
        [DataMember]
        public int Models { get; set; }

        /// <summary>
        /// Gets or sets the total entries (elements) under the model collection.
        /// </summary>
        [DataMember]
        public int Entries { get; set; }
    }
}