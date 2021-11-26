/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for <see cref="HttpRequest"/> object and other related object.
    /// </summary>
    public static class RequestExtensions
    {
        /// <summary>
        /// Gets a Gravity API Authentication object based on request authorization header.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> to get authorization header from.</param>
        /// <returns>Gravity API Authentication object</returns>
        public static Authentication GetAuthentication(this HttpRequest request)
        {
            // get header
            var header = $"{request.Headers["Authorization"]}";

            // get token
            var onToken = Regex.Match(header, @"(?i)(?<=Basic\s+)[^\s](.+?)+").Value.FromBase64();
            var credentials = onToken.Split(":").Where(i => !string.IsNullOrEmpty(i)).ToArray();

            // no credentials
            if (credentials.Length == 0)
            {
                return new Authentication
                {
                    Password = string.Empty,
                    UserName = string.Empty
                };
            }

            // only user
            if (credentials.Length == 1)
            {
                return new Authentication { UserName = credentials[0] };
            }

            // get object
            return new Authentication
            {
                Password = credentials[1],
                UserName = credentials[0]
            };
        }

        /// <summary>
        /// Converts <see cref="int"/> value to equivalent <see cref="int"/>.
        /// </summary>
        /// <param name="statusCode"><see cref="int"/> to convert from.</param>
        /// <returns>Status code as <see cref="int"/>.</returns>
        public static int ToInt32(this int statusCode)
        {
            _ = int.TryParse($"{statusCode}", out int statusCodeOut);
            return statusCodeOut;
        }

        /// <summary>
        /// Serialize an object to Json and set it as the request body.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="request">The <see cref="HttpRequest"/> to set body.</param>
        /// <param name="body">The body object to set.</param>
        /// <returns>Self reference with new <see cref="HttpRequest.Body"/>.</returns>
        public static HttpRequest SetBody<T>(this HttpRequest request, T body)
        {
            // setup
            var bodyContent = JsonSerializer.Serialize(body, ControllerUtilities.JsonSettings);
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(string.IsNullOrEmpty(bodyContent) ? "{}" : bodyContent));

            // get
            return request;
        }

        #region *** Read Request ***
        /// <summary>
        /// Deserialize a <see cref="HttpRequest.Body"/> into an object of a type.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to deserialize the <see cref="HttpRequest"/> to.</typeparam>
        /// <param name="request">The <see cref="HttpRequest"/> to deserialize.</param>
        /// <returns>An object of the given type.</returns>
        public static async Task<T> ReadAsAsync<T>(this HttpRequest request)
        {
            // read content
            var requestBody = await DoReadAsync(request).ConfigureAwait(false);

            // exit conditions
            if (!requestBody.IsJson())
            {
                throw new NotSupportedException("The request body must be JSON formatted.");
            }

            // deserialize into object
            return JsonSerializer.Deserialize<T>(requestBody);
        }

        /// <summary>
        /// Reads a <see cref="HttpRequest.Body"/> object.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to read.</param>
        /// <returns>The <see cref="HttpRequest.Body"/> as <see cref="string"/>.</returns>
        public static Task<string> ReadAsync(this HttpRequest request)
        {
            return DoReadAsync(request);
        }

        private static async Task<string> DoReadAsync(HttpRequest request)
        {
            using var streamReader = new StreamReader(request.Body);
            return await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }
        #endregion
    }
}
