using Newtonsoft.Json;

using System.Runtime.Serialization;

namespace Rhino.Agent.Models
{
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
            return JsonConvert.SerializeObject(this);
        }
    }
}
