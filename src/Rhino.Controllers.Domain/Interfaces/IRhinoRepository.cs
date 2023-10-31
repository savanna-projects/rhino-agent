/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Models.Server;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IRhinoRepository : IHasAuthentication<IRhinoRepository>
    {
        GenericResultModel<RhinoTestRun> InvokeConfiguration(RhinoConfiguration configuration);
        GenericResultModel<RhinoTestRun> InvokeConfiguration(string configuration);
        RhinoTestRun InvokeConfiguration(string configuration, string spec);
        RhinoTestRun InvokeConfiguration(IDictionary<string, object> driverParams, string spec);
        IEnumerable<GenericResultModel<RhinoTestRun>> InvokeCollection(string collection, bool isParallel, int maxParallel);
    }
}