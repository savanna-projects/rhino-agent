/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Contract for api/:version/models/types operator(s).
    /// </summary>
    [DataContract]
    public class RhinoModelTypeModel : BaseModel<object>
    { }
}