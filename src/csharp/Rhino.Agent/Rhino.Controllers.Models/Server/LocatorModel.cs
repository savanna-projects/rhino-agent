/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models
{
    [DataContract]
    public class LocatorModel : BaseModel<object>
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