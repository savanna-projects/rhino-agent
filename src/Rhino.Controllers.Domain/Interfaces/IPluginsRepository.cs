/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IPluginsRepository : IRepository<string>
    {
        string Add(IEnumerable<string> entity, bool isPrivate);
    }
}
