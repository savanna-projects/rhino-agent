/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Interfaces;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IApplicationRepository : ICrudable<RhinoTestCase>
    {
        RhinoConnectorConfiguration Configuration { get; }
        IConnector Connector { get; }
        IApplicationRepository SetConnector(RhinoConnectorConfiguration configuration);
    }
}
