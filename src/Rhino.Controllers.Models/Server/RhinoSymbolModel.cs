/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Represents programming constructs like variables, classes, interfaces etc. that appear in a document.
    /// Document symbols can be hierarchical and they have two ranges: one that encloses its definition
    /// and one that points to its most interesting range, e.g. the range of an identifier.
    /// </summary>
    [DataContract]
    public class RhinoSymbolModel
    {
        /// <summary>
        /// Gets or sets the name of this symbol.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets more detail for this symbol, e.g. the signature of a function.
        /// </summary>
        [DataMember]
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the type of this symbol, e.g. `Function`, `Module`, etc.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the code line number of this symbol.
        /// </summary>
        [DataMember]
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the range enclosing this symbol not including leading/trailing whitespace
        /// but everything else, e.g. comments and code.
        /// </summary>
        [DataMember]
        public RhinoSymbolRangeModel Range { get; set; }

        /// <summary>
        /// Gets or sets the range that should be selected and reveal when this symbol is being
        /// picked, e.g. the name of a function.
        /// </summary>
        [DataMember]
        public RhinoSymbolRangeModel SelectedRange { get; set; }

        /// <summary>
        /// Gets or sets a collection of extra annotations that tweak the rendering of a symbol.
        /// </summary>
        [DataMember]
        public IEnumerable<int> Tags { get; set; }

        /// <summary>
        /// Nested symbols of this symbol, e.g. properties of a class.
        /// </summary>
        public IEnumerable<RhinoSymbolModel> Symbols { get; set; }
    }
}
