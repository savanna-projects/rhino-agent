/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhino.Controllers.Models
{
    /// <summary>
    /// Contract for api/:version/:controller general error message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class GenericErrorModel<T> : IErrorResponseProvider
    {
        // errors map
        private readonly IDictionary<int, (string Link, string Title)> statuCodeMap = new Dictionary<int, (string Link, string Message)>
        {
            [StatusCodes.Status400BadRequest] = ("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Malformed request syntax, invalid request message framing, or deceptive request routing."),
            [StatusCodes.Status404NotFound] = ("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Resource not found."),
            [StatusCodes.Status500InternalServerError] = ("https://tools.ietf.org/html/rfc7231#section-6.6.1", "The server encountered an unexpected condition that prevented it from fulfilling the request.")
        };

        /// <summary>
        /// Gets or sets the status code of the response.
        /// </summary>
        [DataMember]
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the type of the response (can be a reference or URL).
        /// </summary>
        [DataMember]
        public string Type => statuCodeMap[Status == default || !statuCodeMap.ContainsKey(Status) ? StatusCodes.Status500InternalServerError : Status].Link;

        /// <summary>
        /// Gets or sets the title of the response.
        /// </summary>
        [DataMember]
        public string Title => statuCodeMap[Status == default || !statuCodeMap.ContainsKey(Status) ? StatusCodes.Status500InternalServerError : Status].Title;

        /// <summary>
        /// Gets or sets a trace identifier for this response.
        /// </summary>
        [DataMember]
        public string TraceId { get; set; } = DateTime.Now.ToString("yyyyMMdd-HHmmss-fffffff");

        /// <summary>
        /// Gets or sets a list of errors.
        /// </summary>
        [DataMember]
        public ErrorModel Errors { get; set; }

        /// <summary>
        /// Gets or sets the route of the request which leads to the response.
        /// </summary>
        [DataMember]
        public IDictionary<string, object> RouteData { get; set; }

        /// <summary>
        /// Gets or sets the request object which leads to the response.
        /// </summary>
        [DataMember]
        public T Request { get; set; }

        /// <summary>
        /// Creates and returns a new error response given the provided context.
        /// </summary>
        /// <param name="context">The error context used to generate response.</param>
        /// <returns>The generated response.</returns>
        public IActionResult CreateResponse(ErrorResponseContext context)
        {
            // setup
            Status = context.StatusCode;
            var errors = new List<string> { context.ErrorCode };

            // build: errors list
            if (!string.IsNullOrEmpty(context.Message))
            {
                errors.Add(context.Message);
            }
            if (!string.IsNullOrEmpty(context.MessageDetail))
            {
                errors.Add(context.MessageDetail);
            }

            // build
            var error = new
            {
                Errors = errors,
                RouteData = context.Request.RouteValues,
                Status,
                Type,
                Title,
                TraceId
            };

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(error),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = Status
            };
        }

        /// <summary>
        /// Nested class for special error format.
        /// </summary>
        [DataContract]
        public class ErrorModel
        {
            /// <summary>
            /// Gets or sets a list of errors.
            /// </summary>
            [DataMember, JsonPropertyName("$")]
            public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();
        }
    }
}