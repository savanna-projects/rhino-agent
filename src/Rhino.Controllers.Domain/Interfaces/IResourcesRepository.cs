using Rhino.Controllers.Models.Server;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IResourcesRepository
    {
        (int StatusCode, ResourceFileModel Entity) Create(ResourceFileModel entity);

        int Delete();

        int Delete(string id);

        IEnumerable<ResourceFileModel> Get();

        (int StatusCode, ResourceFileModel Entity) Get(string id);
    }
}
