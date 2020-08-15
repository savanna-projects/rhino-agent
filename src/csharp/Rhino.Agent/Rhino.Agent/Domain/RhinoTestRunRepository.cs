/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;

namespace Rhino.Agent.Domain
{
    /// <summary>
    /// Data Access Layer for Rhino API test runs repository.
    /// </summary>
    public class RhinoTestRunRepository : Repository
    {
        /// <summary>
        /// Creates a new instance of this Rhino.Agent.Domain.Repository.
        /// </summary>
        /// <param name="provider"><see cref="IServiceProvider"/> to use with this Rhino.Agent.Domain.RhinoTestRunRepository.</param>
        public RhinoTestRunRepository(IServiceProvider provider)
            : base(provider)
        { }
    }
}
