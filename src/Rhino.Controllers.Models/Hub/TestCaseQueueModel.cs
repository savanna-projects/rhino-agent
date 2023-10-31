/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Interfaces;

using System;

namespace Rhino.Controllers.Models
{
    public class TestCaseQueueModel
    {
        public IConnector Connector { get; set; }

        public DateTime RegisterationTime { get; set; }

        public RhinoTestCase TestCase { get; set; }

        public WorkerQueueModel Worker { get; set; }
    }
}
