/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        // members: constants
        private static JsonSerializerSettings JsonSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        // TODO: move to global error handling controller (api/:version/error)
        #region *** Error Result   ***
        /// <summary>
        /// Gets an error result with status code 400 (Bad Request).
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return error result.</param>
        /// <param name="message">The message which will be send with the error result.</param>
        /// <returns>Action method result.</returns>
        public static Task<IActionResult> ErrorResultAsync(this ControllerBase controller, string message)
        {
            return DoErrorResultsAsync(controller, message, HttpStatusCode.BadRequest, JsonSettings);
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
            return DoErrorResultsAsync(controller, message, statusCode, JsonSettings);
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
            return DoErrorResultsAsync(controller, message, statusCode, jsonSettings);
        }

        private static async Task<IActionResult> DoErrorResultsAsync(
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
        #endregion

        #region *** Content Result ***
        /// <summary>
        /// Gets an error result with status code 400 (Bad Request).
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return error result.</param>
        /// <param name="responseBody">The object which will be send with the result (response data).</param>
        /// <returns>Action method result.</returns>
        public static IActionResult ContentResult(this ControllerBase controller, object responseBody)
        {
            return DoContentResult(controller, responseBody, HttpStatusCode.OK, JsonSettings);
        }

        /// <summary>
        /// Gets an error result with status code 400 (Bad Request).
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return error result.</param>
        /// <param name="responseBody">The object which will be send with the result (response data).</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> which will be send with the error result.</param>
        /// <returns>Action method result.</returns>
        public static IActionResult ContentResult(this ControllerBase controller, object responseBody, HttpStatusCode statusCode)
        {
            return DoContentResult(controller, responseBody, statusCode, JsonSettings);
        }

        /// <summary>
        /// Gets an error result with status code 400 (Bad Request).
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return error result.</param>
        /// <param name="responseBody">The object which will be send with the result (response data).</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> which will be send with the error result.</param>
        /// <param name="jsonSettings">The settings by which to serialize the response body.</param>
        /// <returns>Action method result.</returns>
        public static IActionResult ContentResult(
            this ControllerBase controller, object responseBody, HttpStatusCode statusCode, JsonSerializerSettings jsonSettings)
        {
            return DoContentResult(controller, responseBody, statusCode, jsonSettings);
        }

        private static IActionResult DoContentResult(
            ControllerBase controller, object responseBody, HttpStatusCode statusCode, JsonSerializerSettings jsonSettings)
        {
            // setup
            statusCode = statusCode.ToInt32() > 400 ? HttpStatusCode.OK : statusCode;

            // response
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(responseBody, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode.ToInt32()
            };
        }
        #endregion
    }
}
