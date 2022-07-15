/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IMetaDataRepository : IHasAuthentication<IMetaDataRepository>
    {
        IEnumerable<PropertyModel> GetAnnotations();
        IEnumerable<AssertModel> GetAssertions();
        IEnumerable<ConnectorModel> GetConnectors();
        IEnumerable<DriverModel> GetDrivers();
        IEnumerable<BaseModel<object>> GetLocators();
        IEnumerable<MacroModel> GetMacros();
        IEnumerable<RhinoModelCollection> GetModels();
        IEnumerable<BaseModel<object>> GetModelTypes();
        IEnumerable<OperatorModel> GetOperators();
        IEnumerable<ActionModel> GetPlugins();
        IEnumerable<ActionModel> GetPlugins(string configuration);
        IEnumerable<ReporterModel> GetReporters();
        IEnumerable<ServiceEventModel> GetServiceEvents();
        IEnumerable<string> GetServices();
        IEnumerable<RhinoVerbModel> GetVerbs();
        Task<string> GetVersionAsync();
        string GetTestTree(string rhinoTestCase);
    }
}
