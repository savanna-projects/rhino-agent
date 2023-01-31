/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Contract for api/:version/gravity/convert action(s).
    /// </summary>
    [DataContract]
    public class ActionRuleConvertModel
    {
        /// <summary>
        /// Gets or sets an a collection of ExternalRepository for invoking/retrieving remote actions.
        /// </summary
        [DataMember]
        public IEnumerable<ExternalRepository> ExternalRepositories { get; set; }

        /// <summary>
        /// Gets or sets the `Rhino Specifications` to convert.
        /// </summary>
        [DataMember, Required]
        public string Action { get; set; }
    }
}
