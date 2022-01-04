/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for _Locators.cshtml page.
    /// </summary>
    [DataContract]
    public class Locators
    {
        /// <summary>
        /// Gets or sets the Alias (will be used in the steps instead of the locator).
        /// </summary>
        [DataMember]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the Query Selector.
        /// </summary>
        [DataMember]
        public string Selector { get; set; }

        /// <summary>
        /// Gets or sets the Short Path (by id attribute if available).
        /// </summary>
        [DataMember]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the Full Path (direct path - ignores attributes).
        /// </summary>
        [DataMember]
        public string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the selected locator.
        /// </summary>
        [DataMember]
        public string SelectedLocator { get; set; }
    }
}
