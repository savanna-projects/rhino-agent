/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    [DataContract]
    public class RunsStatusModel
    {
        [DataMember]
        public IEnumerable<string> Runs { get; set; }

        [DataMember]
        public int TotalPending { get; set; }

        [DataMember]
        public int TotalRunning { get; set; }

        [DataMember]
        public int TotalRuns { get; set; }
    }
}
