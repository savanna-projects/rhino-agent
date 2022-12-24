/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Loader.Contracts
{
    /// <summary>
    /// Event arguments for AssemblyLoaded event.
    /// </summary>
    [DataContract]
    public class AssembliesLoaderLoadEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the <see cref="System.Reflection.Assembly"/> loaded.
        /// </summary>
        [DataMember]
        public Assembly Assembly { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Type"/> listing all the types
        /// in the <see cref="System.Reflection.Assembly"/>.
        /// </summary>
        [DataMember]
        public IEnumerable<Type> Types { get; set; }
    }
}
