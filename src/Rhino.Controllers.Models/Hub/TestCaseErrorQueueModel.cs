/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Rhino.Api.Contracts.AutomationProvider;

using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    [DataContract]
    public class TestCaseErrorQueueModel
    {
        [DataMember]
        public string StackTrace { get; set; }

        [DataMember]
        public RhinoTestCase TestCase { get; set; }

        [DataMember]
        public WorkerQueueModel Worker { get; set; }
    }
}
