/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using System;
using System.IO;
using System.Net;

namespace Rhino.Controllers
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // setup
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Data"));

            // run
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseUrls()
            .ConfigureKestrel(SetOptions)
            .UseStartup<Startup>();

        private static void SetOptions(KestrelServerOptions options)
        {
            const int httpsPort = 9001;
            const int httpPort = 9000;
            const string certPassword = "30908f87-8539-477a-86e7-a4c13d4583c4";
            var certPath = Path.Combine("Certificates", "Rhino.Agent.pfx");

            options.Listen(IPAddress.Any, httpsPort, listenOptions => listenOptions.UseHttps(certPath, certPassword));
            options.Listen(IPAddress.Any, httpPort);
        }
    }
}