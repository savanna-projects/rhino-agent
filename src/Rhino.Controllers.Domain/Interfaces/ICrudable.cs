/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Generic;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface ICrudable<T>
    {
        string Add(T entity);
        int Delete();
        int Delete(string id);
        IEnumerable<T> Get();
        (int StatusCode, T Entity) Get(string id);
        (int StatusCode, T Entity) Update(string id, T entity);
        (int StatusCode, T Entity) Update(string id, IDictionary<string, object> fields);
    }
}