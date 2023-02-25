/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Rhino.Controllers.Domain.Cache;
using Rhino.Controllers.Domain.Interfaces;

using System.Runtime.Serialization;

namespace Rhino.Controllers.Domain
{
    /// <summary>
    /// Contract for caching meta data.
    /// </summary>
    [DataContract]
    public class ApplicationCache
    {
        // members: state
        private readonly IDomain _domain;

        public ApplicationCache(IDomain domain)
        {
            _domain = domain;
        }
    }
}
