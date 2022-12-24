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
    /// Event arguments for AssembliesLoaded event.
    /// </summary>
    [DataContract]
    public class AssembliesLoadedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the collection of <see cref="Assembly"/> and the collection of <see cref="Type"/>
        /// that belongs to the <see cref="Assembly"/>.
        /// </summary>
        [DataMember]
        public IEnumerable<(Assembly Assembly, IEnumerable<Type> Types)> AssembliesCollection { get; set; }
    }
}
