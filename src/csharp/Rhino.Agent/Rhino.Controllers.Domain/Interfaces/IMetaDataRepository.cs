/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IMetaDataRepository : IHasAuthentication<IMetaDataRepository>
    {
        IEnumerable<ActionModel> GetPlugins();
        IEnumerable<AssertModel> GetAssertions();
        IEnumerable<ConnectorModel> GetConnectors();
        IEnumerable<DriverModel> GetDrivers();
        IEnumerable<BaseModel<object>> GetLocators();
        IEnumerable<MacroModel> GetMacros();
        IEnumerable<OperatorModel> GetOperators();
        IEnumerable<ReporterModel> GetReporters();
        Task<string> GetVersionAsync();
        IEnumerable<PropertyModel> GetAnnotations();
        IEnumerable<RhinoModelCollection> GetModels();
        IEnumerable<BaseModel<object>> GetModelTypes();
        IEnumerable<RhinoVerbModel> GetVerbs();
    }
}