/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;
using System.Runtime.Serialization;

namespace Loader.Contracts
{
    /// <summary>
    /// Event arguments for AssemblyLoadError event.
    /// </summary>
    [DataContract]
    public class AssembliesLoaderErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the <see cref="System.Reflection.Assembly"/> file name.
        /// </summary>
        [DataMember]
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the name of the source method under which the error occurs.
        /// </summary>
        [DataMember]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Exception"/> object of the error.
        /// </summary>
        [DataMember]
        public Exception Exception { get; set; }
    }
}
