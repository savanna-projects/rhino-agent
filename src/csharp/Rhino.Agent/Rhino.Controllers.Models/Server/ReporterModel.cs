/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    // TODO: change genetic type to ActionAttribute when TypeId is fixed
    /// <summary>
    /// Contract for api/:version/:meta reporter(s).
    /// </summary>
    [DataContract]
    public class ReporterModel : BaseModel<object>
    { }
}