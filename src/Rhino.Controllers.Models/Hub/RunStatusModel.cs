/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    [DataContract]
    public class RunStatusModel
    {
        [DataMember]
        public int Completed { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public IEnumerable<string> Pending { get; set; }

        [DataMember]
        public double Progress { get; set; }

        [DataMember]
        public IEnumerable<object> Running { get; set; }

        [DataMember]
        public TimeSpan RunningTime { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public int Total { get; set; }

        [DataMember]
        public int TotalPending { get; set; }

        [DataMember]
        public int TotalRunning { get; set; }
    }
}
