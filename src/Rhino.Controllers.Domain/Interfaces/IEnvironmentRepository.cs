/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IEnvironmentRepository : IRepository<KeyValuePair<string, object>>
    {
        IDictionary<string, object> Add(IDictionary<string, object> entity);
        int DeleteByName(string name);
        (int StatusCode, IDictionary<string, object> Entities) Sync();
        (int StatusCode, KeyValuePair<string, object> Entity) GetByName(string name);
    }
}
