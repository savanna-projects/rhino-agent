/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    // TODO: change the generic type to DriverMethodAttribute when available on Gravity.Abstraction release
    /// <summary>
    /// Contract for api/:version/:meta driver(s).
    /// </summary>
    [DataContract]
    public class DriverModel : BaseModel<int>
    { }
}