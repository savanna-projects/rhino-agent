/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Extensions;
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Http;

using System.Net;
using System.Text.RegularExpressions;

namespace Rhino.Agent.Extensions
{
    /// <summary>
    /// Extension package for <see cref="HttpRequest"/> object and other related object.
    /// </summary>
    internal static class RequestExtensions
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
            var credentials = onToken.Split(":");

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
        /// Converts <see cref="HttpStatusCode"/> value to equivalent <see cref="int"/>.
        /// </summary>
        /// <param name="statusCode"><see cref="HttpStatusCode"/> to convert from.</param>
        /// <returns>Status code as <see cref="int"/>.</returns>
        public static int ToInt32(this HttpStatusCode statusCode)
        {
            int.TryParse($"{(int)statusCode}", out int statusCodeOut);
            return statusCodeOut;
        }
    }
}