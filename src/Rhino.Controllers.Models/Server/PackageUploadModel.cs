/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Request contract for api/:version/plugins controller.
    /// </summary>
    [DataContract]
    public class PackageUploadModel
    {
        /// <summary>
        /// Gets or sets the author of the package (separate with comma for multiple authors).
        /// </summary>
        [DataMember]
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the unique package identifier.
        /// </summary>
        [DataMember, Required(ErrorMessage = "You must provide a unique package id")]
        public string Id { get; set; }

        /// <summary>
        /// The file data as Base64 string.
        /// </summary>
        [DataMember, Required(ErrorMessage = "You must provide the package data as Base64 string")]
        public string FileData { get; set; }

        /// <summary>
        /// The open source license for the package.
        /// </summary>
        [DataMember]
        public string Licence { get; set; }

        /// <summary>
        /// Gets or sets package name.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The type of the package to submit.
        /// </summary>
        [DataMember, Required(ErrorMessage = "You must provide a package type (e.g., `Gravity`, `Reporter` or `Connector`)")]
        public string PackageType { get; set; }

        /// <summary>
        /// The link to the project repository.
        /// </summary>
        public string ProjectLink { get; set; }

        /// <summary>
        /// Gets or sets the publish date of the package.
        /// </summary>
        [DataMember]
        public string PublishDate { get; set; } = DateTime.Now.ToString();

        /// <summary>
        /// The package version.
        /// </summary>
        [DataMember, DefaultValue("1.0.0.0")]
        public string Verseion { get; set; } = "1.0.0.0";
    }
}
