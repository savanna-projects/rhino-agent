using Gravity.Services.DataContracts;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    [DataContract]
    public class PluginCacheSyncModel
    {
        [DataMember]
        public Authentication Authentication { get; set; }

        [DataMember]
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must provide a valid `Rhino Specifications`.")]
        public string Specification { get; set; }
    }
}
