/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

using System.Data;
using System.Reflection;

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
    }
}
