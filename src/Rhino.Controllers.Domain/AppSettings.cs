using Microsoft.Extensions.Configuration;

using System.Net;
using System.Net.Sockets;

namespace Rhino.Controllers.Domain
{
    public class AppSettings
    {
        public AppSettings(IConfiguration configuration)
        {
            // setup
            Hub ??= new HubConfiguration();
            Worker ??= new WorkerConfiguration();
            Configuration = configuration;

            // bind
            configuration.GetSection("Rhino:HubConfiguration").Bind(Hub);
            configuration.GetSection("Rhino:WorkerConfiguration").Bind(Hub);
        }

        public IConfiguration Configuration { get; }

        public HubConfiguration Hub { get; }

        public WorkerConfiguration Worker { get; }

        /// <summary>
        /// Gets the local IPv6 address.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
            {
                if (!string.IsNullOrEmpty(ip.ToString())) return ip.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Child configuration
        /// </summary>
        public class HubConfiguration
        {
            public double CreationTimeout { get; set; }
        }

        /// <summary>
        /// Child configuration
        /// </summary>
        public class WorkerConfiguration
        {
            public string HubAddress { get; set; }
            public int HubApiVersion { get; set; }
        }
    }
}
