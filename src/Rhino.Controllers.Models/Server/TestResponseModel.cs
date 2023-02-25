/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Response contract for api/:version/tests controller.
    /// </summary>
    [DataContract]
    public class TestResponseModel
    {
        /// <summary>
        /// Gets or sets the id of the entity in the domain state.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a collection of configurations associated with tests.
        /// </summary>
        [DataMember]
        public IEnumerable<string> Configurations { get; set; }

        /// <summary>
        /// Gets or sets the number of tests in the collection.
        /// </summary>
        [DataMember]
        public int Tests { get; set; }
    }
}