/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Loader.Contracts
{
    /// <summary>
    /// Event arguments for SetupCompleted event.
    /// </summary>
    [DataContract]
    public class AssembliesLoaderSetupEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the base directory from which the assemblies are loading.
        /// </summary>
        [DataMember]
        public string Directory { get; set; }

        /// <summary>
        /// Gets a collction of <see cref="string"/> listing the additional
        /// directories from which assemblies are loading.
        /// </summary>
        [DataMember]
        public IEnumerable<string> Directories { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="string"/> listing all the assemblies
        /// files found.
        /// </summary>
        [DataMember]
        public IEnumerable<string> Files { get; set; }
    }
}
