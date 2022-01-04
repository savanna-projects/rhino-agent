/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IRhinoRepository : IHasAuthentication<IRhinoRepository>
    {
        (int StatusCode, RhinoTestRun TestRun) InvokeConfiguration(RhinoConfiguration configuration);
        (int StatusCode, RhinoTestRun TestRun) InvokeConfiguration(string configuration);
        RhinoTestRun InvokeConfiguration(string configuration, string spec);
        RhinoTestRun InvokeConfiguration(IDictionary<string, object> driverParams, string spec);
        IEnumerable<(int StatusCode, RhinoTestRun TestRun)> InvokeCollection(string collection, bool isParallel, int maxParallel);
    }
}