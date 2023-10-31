using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Models;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IRhinoAsyncRepository: IHasAuthentication<IRhinoAsyncRepository>
    {
        AsyncInvokeModel StartConfiguration(RhinoConfiguration configuration);
        AsyncInvokeModel StartConfiguration(string configuration);
        IEnumerable<AsyncInvokeModel> StartCollection(string collection, bool isParallel, int maxParallel);
        (int StatusCode, AsyncStatusModel<RhinoConfiguration> Status) GetStatus(Guid id);
        IEnumerable<AsyncStatusModel<RhinoConfiguration>> GetStatus();
        int Delete(string id);
        int Delete();
    }
}