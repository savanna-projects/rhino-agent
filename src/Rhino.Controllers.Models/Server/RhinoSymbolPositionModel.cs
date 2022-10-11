/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Represents a line and character position, such as the position of the cursor.
    /// </summary>
    [DataContract]
    public struct RhinoSymbolPositionModel
    {
        /// <summary>
        /// Initialize a new instance of RhinoSymbolPositionModel.
        /// </summary>
        /// <param name="character">Zero-based character value.</param>
        /// <param name="line">Zero-based line value.</param>
        public RhinoSymbolPositionModel(int character, int line)
        {
            Character = character;
            Line = line;
        }

        /// <summary>
        /// Gets or sets the zero-based character value.
        /// </summary>
        [DataMember]
        public int Character { get; }

        /// <summary>
        /// Gets or sets the zero-based line value.
        /// </summary>
        [DataMember]
        public int Line { get; }
    }
}
