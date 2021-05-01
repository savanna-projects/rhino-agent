/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet.Engine.Attributes;

using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for api/:version/:meta macro(s).
    /// </summary>
    [DataContract]
    public class MacroModel : BaseModel<MacroAttribute>
    {
        // TODO: implement object to MD to allow dynamic documentation generator and remove documentation redundancy.
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return base.ToString();
        }
    }
}