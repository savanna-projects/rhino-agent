/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    [DataContract]
    public class PluginsCacheModel
    {
        [DataMember]
        public IEnumerable<ActionModel> ActionsCache { get; set; }

        [DataMember]
        public IEnumerable<ActionModel> ActionsCacheByConfiguration { get; set; }

        [DataMember]
        public IEnumerable<PluginCacheModel> PluginsCache { get; set; }
    }
}
