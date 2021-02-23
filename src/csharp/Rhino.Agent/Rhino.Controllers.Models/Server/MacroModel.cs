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
    /// Contract for api/:version/:meta macro(s).
    /// </summary>
    [DataContract]
    public class MacroModel : BaseModel<object>
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