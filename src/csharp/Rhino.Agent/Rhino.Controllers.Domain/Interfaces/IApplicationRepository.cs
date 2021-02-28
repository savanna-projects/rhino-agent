/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IApplicationRepository : ICrudable<RhinoTestCase>
    {
        RhinoConnectorConfiguration Configuration { get; }
        IApplicationRepository SetConnector(RhinoConnectorConfiguration configuration);
    }
}
