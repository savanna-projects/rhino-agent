/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

using System.Diagnostics;

namespace Rhino.Controllers.Extensions
{
    public static class DotnetExtensions
    {
        /// <summary>
        /// Gets the remote address of a connection.
        /// </summary>
        /// <param name="context">The HubCallerContext to get address from.</param>
        /// <returns>The remote address with the port.</returns>
        public static (string Address, int Port) GetAddress(this HubCallerContext context)
        {
            // setup
            var feature = context.Features.Get<IHttpConnectionFeature>();
            var remoteAddress = $"{feature?.RemoteIpAddress}";
            var ip = $"{(remoteAddress.Equals("::1") ? "localhost" : remoteAddress)}";
            var port = feature == default ? 0 : feature.RemotePort;

            // get
            return (ip, port);
        }

        /// <summary>
        /// Sends an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="client">The HTTP client to send with.</param>
        /// <param name="requestUri">The Uri the request is send to.</param>
        public static async Task< HttpResponseMessage> GetAsync(this HttpClient client, string requestUri, TimeSpan timeout)
        {
            // setup
            var onTimeout = DateTime.Now.Add(timeout);

            // retry
            while (DateTime.Now < onTimeout)
            {
                try
                {
                    return await client.GetAsync(requestUri);
                }
                catch (Exception e) when (e != null)
                {
                    var message = $"Send-HttpRequest = (Error | {e.GetBaseException().Message} | {requestUri})";
                    Debug.WriteLine(message);
                    Console.WriteLine(message);
                    Trace.TraceWarning(message);
                }
                await Task.Delay(3000);
            }

            // default
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                ReasonPhrase = $"Send-HttpRequest = (Timeout | {timeout})"
            };
        }
    }
}
