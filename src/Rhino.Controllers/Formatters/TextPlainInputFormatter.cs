/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Mvc.Formatters;

using Rhino.Controllers.Extensions;

using System.Net.Mime;
using System.Threading.Tasks;

namespace Rhino.Controllers.Formatters
{
    /// <summary>
    /// Formatter to read text/plain from micro-service signature
    /// </summary>
    public class TextPlainInputFormatter : InputFormatter
    {
        // constants
        private const string ContentType = MediaTypeNames.Text.Plain;

        /// <summary>
        /// Creates a new instance of <see cref="InputFormatter"/>.
        /// </summary>
        public TextPlainInputFormatter()
        {
            SupportedMediaTypes.Add(ContentType);
        }

        /// <summary>
        /// Reads an object from the request body.
        /// </summary>
        /// <param name="context">The <see cref="InputFormatterContext"/>.</param>
        /// <returns>A <see cref="Task"/> that on completion deserializes the request body.</returns>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            // build
            var request = await context.HttpContext.Request.ReadAsync().ConfigureAwait(false);

            // get
            return await InputFormatterResult.SuccessAsync(request).ConfigureAwait(false);
        }

        public override bool CanRead(InputFormatterContext context)
        {
            // setup
            var contentType = context.HttpContext.Request.ContentType;

            // get
            return contentType?.StartsWith(ContentType) == true;
        }
    }
}
