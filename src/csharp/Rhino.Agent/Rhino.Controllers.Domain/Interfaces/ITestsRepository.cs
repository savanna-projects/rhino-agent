/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Controllers.Models;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface ITestsRepository : IRepository<RhinoTestCollection>
    {
        public (int statusCode, RhinoTestCollection data) Update(string id, string configuration);

        public (int statusCode, RhinoTestCollection data) Update(string id, RhinoTestModel entity);
    }
}