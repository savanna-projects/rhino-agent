/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

// ignore extended object warning when it's parameter is not used.
# pragma warning disable S1172, RCS1163, IDE0060
namespace Rhino.Agent.Extensions
{
    /// <summary>
    /// Extension package for <see cref="ControllerBase"/> object and other related object.
    /// </summary>
    internal static class ControllerExtensions
    {
        #region *** Error Result   ***
        /// <summary>
        /// Gets an error result with status code 400 (Bad Request).
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return error result.</param>
        /// <param name="message">The message which will be send with the error result.</param>
        /// <returns>Action method result.</returns>
        public static Task<IActionResult> ErrorResultAsync(this ControllerBase controller, string message)
        {
            return DoMessageResultsAsync(controller, message, HttpStatusCode.BadRequest, Startup.JsonSettings);
        }

        /// <summary>
        /// Gets an error result with status code 400 (Bad Request).
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return error result.</param>
        /// <param name="message">The message which will be send with the error result.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> which will be send with the error result.</param>
        /// <returns>Action method result.</returns>
        public static Task<IActionResult> ErrorResultAsync(
            this ControllerBase controller, string message, HttpStatusCode statusCode)
        {
            // setup
            statusCode = statusCode.ToInt32() < 400 ? HttpStatusCode.BadRequest : statusCode;

            // results
            return DoMessageResultsAsync(controller, message, statusCode, Startup.JsonSettings);
        }

        /// <summary>
        /// Gets an error result with status code 400 (Bad Request).
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return error result.</param>
        /// <param name="message">The message which will be send with the error result.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> which will be send with the error result.</param>
        /// <param name="jsonSettings">The settings by which to serialize the response body.</param>
        /// <returns>Action method result.</returns>
        public static Task<IActionResult> ErrorResultAsync(
            this ControllerBase controller, string message, HttpStatusCode statusCode, JsonSerializerSettings jsonSettings)
        {
            // setup
            statusCode = statusCode.ToInt32() < 400 ? HttpStatusCode.BadRequest : statusCode;

            // results
            return DoMessageResultsAsync(controller, message, statusCode, jsonSettings);
        }
        #endregion

        #region *** Content Result ***
        /// <summary>
        /// Gets a result with status code.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return result.</param>
        /// <param name="responseBody">The object which will be send with the result (response data).</param>
        /// <returns>Action method result.</returns>
        public static IActionResult ContentResult(this ControllerBase controller, object responseBody)
        {
            return DoContentResult(controller, responseBody, HttpStatusCode.OK, MediaTypeNames.Application.Json, Startup.JsonSettings);
        }

        /// <summary>
        /// Gets a result with status code.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return result.</param>
        /// <param name="responseBody">The object which will be send with the result (response data).</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> which will be send with the result.</param>
        /// <returns>Action method result.</returns>
        public static IActionResult ContentResult(this ControllerBase controller, object responseBody, HttpStatusCode statusCode)
        {
            return DoContentResult(controller, responseBody, statusCode, MediaTypeNames.Application.Json, Startup.JsonSettings);
        }

        /// <summary>
        /// Gets a result with status code.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return result.</param>
        /// <param name="responseBody">The object which will be send with the result (response data).</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> which will be send with the result.</param>
        /// <param name="jsonSettings">The settings by which to serialize the response body.</param>
        /// <returns>Action method result.</returns>
        public static IActionResult ContentResult(
            this ControllerBase controller, object responseBody, HttpStatusCode statusCode, JsonSerializerSettings jsonSettings)
        {
            return DoContentResult(controller, responseBody, statusCode, MediaTypeNames.Application.Json, jsonSettings);
        }

        /// <summary>
        /// Gets a result with status code.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return result.</param>
        /// <param name="responseBody">The object which will be send with the result (response data).</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> which will be send with the result.</param>
        /// <returns>Action method result.</returns>
        public static IActionResult ContentTextResult(
            this ControllerBase controller, string responseBody, HttpStatusCode statusCode)
        {
            return DoContentResult(controller, responseBody, statusCode, string.Empty, default);
        }
        #endregion

        #region *** Message Result ***
        /// <summary>
        /// Gets a message result with status code.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return message result.</param>
        /// <param name="message">The message which will be send with the message result.</param>
        /// <returns>Action method result.</returns>
        public static Task<IActionResult> MessageResultAsync(this ControllerBase controller, string message)
        {
            return DoMessageResultsAsync(controller, message, HttpStatusCode.BadRequest, Startup.JsonSettings);
        }

        /// <summary>
        /// Gets a message result with status code.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return message result.</param>
        /// <param name="message">The message which will be send with the message result.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> which will be send with the message result.</param>
        /// <returns>Action method result.</returns>
        public static Task<IActionResult> MessageResultAsync(
            this ControllerBase controller, string message, HttpStatusCode statusCode)
        {
            return DoMessageResultsAsync(controller, message, statusCode, Startup.JsonSettings);
        }

        /// <summary>
        /// Gets a message result with status code.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return message result.</param>
        /// <param name="message">The message which will be send with the message result.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> which will be send with the message result.</param>
        /// <param name="jsonSettings">The settings by which to serialize the response body.</param>
        /// <returns>Action method result.</returns>
        public static Task<IActionResult> MessageResultAsync(
            this ControllerBase controller, string message, HttpStatusCode statusCode, JsonSerializerSettings jsonSettings)
        {
            return DoMessageResultsAsync(controller, message, statusCode, jsonSettings);
        }
        #endregion

        private static async Task<IActionResult> DoMessageResultsAsync(
            ControllerBase controller, string message, HttpStatusCode statusCode, JsonSerializerSettings jsonSettings)
        {
            // setup
            statusCode = statusCode.ToInt32() < 400 ? HttpStatusCode.BadRequest : statusCode;
            var requestBody = await controller.Request.ReadAsync().ConfigureAwait(false);
            var requestObject = JsonConvert.DeserializeObject<object>(requestBody);

            // response object
            var obj = new
            {
                Message = message,
                RouteData = controller.RouteData.Values,
                Request = requestObject
            };

            // response
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(obj, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode.ToInt32()
            };
        }

        private static IActionResult DoContentResult(
            ControllerBase controller, object responseBody, HttpStatusCode statusCode, string mediaType, JsonSerializerSettings jsonSettings)
        {
            // no content
            if(responseBody == default)
            {
                return new ContentResult
                {
                    ContentType = MediaTypeNames.Application.Json,
                    StatusCode = statusCode.ToInt32()
                };
            }

            // json
            if (mediaType == MediaTypeNames.Application.Json)
            {
                return new ContentResult
                {
                    Content = JsonConvert.SerializeObject(responseBody, jsonSettings),
                    ContentType = MediaTypeNames.Application.Json,
                    StatusCode = statusCode.ToInt32()
                };
            }

            // text
            return new ContentResult
            {
                Content = $"{responseBody}",
                ContentType = MediaTypeNames.Text.Plain,
                StatusCode = statusCode.ToInt32()
            };
        }
    }
}
