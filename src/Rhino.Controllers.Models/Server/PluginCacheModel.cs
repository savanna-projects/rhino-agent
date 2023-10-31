/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Rhino.Api.Contracts.AutomationProvider;

using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    [DataContract]
    public class PluginCacheModel
    {
        [DataMember]
        public ActionModel ActionModel { get; set; }

        [DataMember]
        public string Directory { get; set; }

        [DataMember]
        public ExternalRepository Repository { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public RhinoPlugin Plugin { get; set; }

        [DataMember]
        public string Specifications { get; set; }
    }
}
