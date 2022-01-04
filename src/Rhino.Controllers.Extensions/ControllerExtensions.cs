/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Controllers.Models;

using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for <see cref="ControllerBase"/> object and other related object.
    /// </summary>
    public static class ControllerExtensions
    {
        #region *** Error Result   ***
        /// <summary>
        /// Gets an error result with status code 400 (Bad Request).
        /// </summary>
        /// <typeparam name="T">The type of the request message.</typeparam>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return error result.</param>
        /// <param name="message">The message which will be send with the error result.</param>
        /// <returns>Action method result.</returns>
        public static Task<IActionResult> ErrorResultAsync<T>(this ControllerBase controller, string message)
        {
            return DoErrorResultAsync<T>(controller, message, StatusCodes.Status400BadRequest, ControllerUtilities.JsonSettings);
        }

        /// <summary>
        /// Gets an error result with status code 400 (Bad Request).
        /// </summary>
        /// <typeparam name="T">The type of the request message.</typeparam>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return error result.</param>
        /// <param name="message">The message which will be send with the error result.</param>
        /// <param name="statusCode">The <see cref="int"/> which will be send with the error result.</param>
        /// <returns>Action method result.</returns>
        public static Task<IActionResult> ErrorResultAsync<T>(
            this ControllerBase controller, string message, int statusCode)
        {
            // setup
            statusCode = statusCode.ToInt32() < 400 ? StatusCodes.Status400BadRequest : statusCode;

            // results
            return DoErrorResultAsync<T>(controller, message, statusCode, ControllerUtilities.JsonSettings);
        }

        /// <summary>
        /// Gets an error result with status code 400 (Bad Request).
        /// </summary>
        /// <typeparam name="T">The type of the request message.</typeparam>
        /// <param name="controller">The <see cref="ControllerBase"/> on which to return error result.</param>
        /// <param name="message">The message which will be send with the error result.</param>
        /// <param name="statusCode">The <see cref="int"/> which will be send with the error result.</param>
        /// <param name="jsonSettings">The settings by which to serialize the response body.</param>
        /// <returns>Action method result.</returns>
        public static Task<IActionResult> ErrorResultAsync<T>(
            this ControllerBase controller, string message, int statusCode, JsonSerializerOptions jsonSettings)
        {
            // setup
            statusCode = statusCode.ToInt32() < 400 ? StatusCodes.Status400BadRequest : statusCode;

            // results
            return DoErrorResultAsync<T>(controller, message, statusCode, jsonSettings);
        }

        private static async Task<IActionResult> DoErrorResultAsync<T>(ControllerBase controller, string message, int statusCode, JsonSerializerOptions jsonSettings)
        {
            // setup
            var requestBody = await controller.Request.ReadAsync().ConfigureAwait(false);
            var entity = !string.IsNullOrEmpty(requestBody) && requestBody.IsJson()
                ? JsonSerializer.Deserialize<T>(requestBody)
                : default;

            // build
            var responseBody = new GenericErrorModel<T>
            {
                Errors = new GenericErrorModel<T>.ErrorModel { Errors = new[] { message } },
                Request = entity,
                RouteData = controller.RouteData.Values,
                Status = statusCode
            };

            // get
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(responseBody, jsonSettings),
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode
            };
        }
        #endregion
    }
}
