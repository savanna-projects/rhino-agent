/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    [DataContract]
    public class WorkerQueueModel
    {
        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public int Port { get; set; }
    }
}
