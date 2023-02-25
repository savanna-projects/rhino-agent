/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Swashbuckle.AspNetCore.Annotations;

using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Base model for static data controller.
    /// </summary>
    [DataContract]
    public abstract class BaseModel<T>
    {
        /// <summary>
        /// Gets or sets the action unique identifier.
        /// </summary>
        [DataMember]
        [SwaggerParameter(Description = "The action unique identifier.", Required = true)]
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action literal expression (human format).
        /// </summary>
        [DataMember]
        [SwaggerParameter(Description = "The action literal expression (human format).", Required = true)]
        public string Literal { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the verb used by this action to identify elementToActOn property.
        /// </summary>
        /// <remarks>Verb must be supported by Rhino engine.</remarks>
        [DataMember]
        [SwaggerParameter(Description = "The verb used by this action to identify elementToActOn property.", Required = true)]
        public string Verb { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the raw static data of the entity.
        /// </summary>
        [DataMember]
        [SwaggerParameter(Description = "The raw static data of the entity.", Required = true)]
        public T Entity { get; set; }
    }
}
