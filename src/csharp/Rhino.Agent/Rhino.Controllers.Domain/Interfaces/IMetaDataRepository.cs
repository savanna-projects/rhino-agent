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
        IEnumerable<ActionModel> Plugins();
        IEnumerable<AssertModel> Assertions();
        IEnumerable<ConnectorModel> Connectors();
        IEnumerable<DriverModel> Drivers();
        IEnumerable<LocatorModel> Locators();
        IEnumerable<MacroModel> Macros();
        IEnumerable<OperatorModel> Operators();
        IEnumerable<ReporterModel> Reporters();
        Task<string> GetVersionAsync();
    }
}