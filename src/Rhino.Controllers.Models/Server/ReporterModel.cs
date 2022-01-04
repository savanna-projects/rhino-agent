/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Api.Contracts.Attributes;

using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for api/:version/:meta reporter(s).
    /// </summary>
    [DataContract]
    public class ReporterModel : BaseModel<ReporterAttribute>
    { }
}