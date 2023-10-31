using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    [DataContract]
    public class ResourceFileModel
    {
        [DataMember, Required]
        public string FileName { get; set; }

        [DataMember, Required]
        public string Path { get; set; }

        [DataMember, Required]
        public string Content { get; set; }
    }
}
