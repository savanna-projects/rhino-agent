/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.IO;
using System.IO.Compression;

namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for <see cref="Stream"/> object and other related objects.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Zip a file into <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">MemoryStream with original file content.</param>
        /// <param name="fileName">Name of the file in the ZIP container.</param>
        /// <returns>byte array of zipped file.</returns>
        public static byte[] Zip(this MemoryStream stream, string fileName)
        {
            // setup
            using var zipStream = new MemoryStream();
            using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);

            // create zip entry
            var zipEntry = zipArchive.CreateEntry(fileName + ".zip");

            // write to new stream
            using (var writer = new StreamWriter(zipEntry.Open()))
            {
                stream.WriteTo(writer.BaseStream);
            }

            // return zip bytes
            return zipStream.ToArray();
        }
    }
}