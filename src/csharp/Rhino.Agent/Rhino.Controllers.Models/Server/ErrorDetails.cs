/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;
using System.Text.Json;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for a generic error message.
    /// </summary>
    [DataContract]
    public class ErrorDetails
    {
        /// <summary>
        /// Gets or sets the status code of this error.
        /// </summary>
        [DataMember]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the message of this error.
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the exception stack of this error.
        /// </summary>
        [DataMember]
        public string Stack { get; set; }

        /// <summary>
        /// Returns this instance of System.String; no actual conversion is performed.
        /// </summary>
        /// <returns>The current string.</returns>
        public override string ToString()
        {
            return DoToString(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        /// <summary>
        /// Returns this instance of System.String; no actual conversion is performed.
        /// </summary>
        /// <returns>The current string.</returns>
        public string ToString(JsonSerializerOptions options)
        {
            return DoToString(options);
        }

        private string DoToString(JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize(this, options);
        }
    }
}
