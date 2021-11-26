/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet.Engine.Core;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;

using System.Net;

namespace Rhino.Controllers.Extensions
{
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Sets programmatic configuration of Kestrel-specific features for Rhino.Api.
        /// </summary>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> to use.</param>
        /// <param name="args">The command line arguments passed by the use when running the application.</param>
        /// <returns>Self reference.</returns>
        public static IWebHostBuilder SetRhinoKestrel(this IWebHostBuilder builder, string[] args)
        {
            // setup
            var cli = "{{$ " + string.Join(" ", args) + "}}";
            var arguments = new CliFactory(cli).Parse();

            // invoke
            builder.ConfigureKestrel(options => SetOptions(options, arguments));

            // get
            return builder;
        }

        private static void SetOptions(KestrelServerOptions options, IDictionary<string, string> arguments)
        {
            // constants
            const int httpsPort = 9001;
            const int httpPort = 9000;
            const string Certificate = "cert";

            // setup
            var cert = arguments.ContainsKey(Certificate)
                ? arguments[Certificate].Split("::")
                : Array.Empty<string>();
            var isCert = cert.Length == 2;

            // build
            var certPassword = isCert ? cert[1] : "30908f87-8539-477a-86e7-a4c13d4583c4";
            var certPath = Path.Combine("Certificates", isCert ? cert[0] : "Rhino.Agent.pfx");

            options.Listen(IPAddress.Any, httpsPort, listenOptions => listenOptions.UseHttps(certPath, certPassword));
            options.Listen(IPAddress.Any, httpPort);
        }

        /// <summary>
        /// Enables static file serving for the given request path.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="physicalPath">The physical folder path to serve.</param>
        /// <param name="route">The route which will point the physical path.</param>
        /// <returns>Self reference.</returns>
        public static IApplicationBuilder UseStaticFiles(this WebApplication app, string physicalPath, string route)
        {
            // force
            Directory.CreateDirectory(physicalPath);

            // setup
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(physicalPath),
                RequestPath = route,
                ServeUnknownFileTypes = true
            });

            // get
            return app;
        }
    }
}
