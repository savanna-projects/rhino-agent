using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Models;
using Rhino.Settings;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IDomain
    {
        IApplicationRepository Application { get; set; }
        AppSettings AppSettings { get; set; }
        IRepository<RhinoConfiguration> Configurations { get; set; }
        IEnvironmentRepository Environments { get; set; }
        IHubRepository Hub { get; }
        ILogsRepository Logs { get; set; }
        IMetaDataRepository MetaData { get; set; }
        IRepository<RhinoModelCollection> Models { get; set; }
        IPluginsRepository Plugins { get; set; }
        IResourcesRepository Resources { get; set; }
        IRhinoRepository Rhino { get; set; }
        IRhinoAsyncRepository RhinoAsync { get; set; }
        ITestsRepository Tests { get; set; }
    }
}
