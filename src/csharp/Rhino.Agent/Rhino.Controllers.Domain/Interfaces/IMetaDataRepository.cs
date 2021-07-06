/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Controllers.Models;

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
        IEnumerable<LocatorModel> GetLocators();
        IEnumerable<MacroModel> GetMacros();
        IEnumerable<OperatorModel> GetOperators();
        IEnumerable<ReporterModel> GetReporters();
        Task<string> GetVersionAsync();
        IEnumerable<PropertyModel> GetAnnotations();
        IEnumerable<RhinoModelCollection> GetModels();
    }
}