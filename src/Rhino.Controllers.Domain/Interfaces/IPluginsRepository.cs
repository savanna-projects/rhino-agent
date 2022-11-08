/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Controllers.Models.Server;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IPluginsRepository : IRepository<string>
    {
        string Add(IEnumerable<string> entity, bool isPrivate);
        Task<(int StatusCode, string Message)> SubmitAsync(PackageUploadModel uploadModel);
    }
}
