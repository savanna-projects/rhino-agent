/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Contract for api/:integration action(s).
    /// </summary>
    [DataContract]
    public class TestCreateModel<T>
    {
        public IEnumerable<string> TestSuites { get; set; }

        public T Spec { get; set; }
    }
}