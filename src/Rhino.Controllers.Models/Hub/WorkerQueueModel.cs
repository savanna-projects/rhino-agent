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
        public DateTime Created { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public DateTime LastHeartbeat { get; set; }

        [DataMember]
        public int Port { get; set; }

        [DataMember]
        public TimeSpan UpTime => DateTime.Now.Subtract(Created);
    }
}
