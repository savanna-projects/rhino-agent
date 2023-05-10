/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    [DataContract]
    public class PluginsCacheModel
    {
        [DataMember]
        public ConcurrentDictionary<string, ActionModel> ActionsCache { get; set; }

        [DataMember]
        public ConcurrentDictionary<string, ActionModel> ActionsCacheByConfiguration { get; set; }

        [DataMember]
        public ConcurrentDictionary<string, PluginCacheModel> PluginsCache { get; set; }
    }
}
