using Microsoft.Extensions.Configuration;

using Rhino.Api.Reporter.Internal;

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
            ReportsAndLogs ??= new ReportConfiguration();
            Worker ??= new WorkerConfiguration();
            Configuration = configuration;

            // bind
            configuration.GetSection("Rhino:HubConfiguration").Bind(Hub);
            configuration.GetSection("Rhino:ReportConfiguration").Bind(ReportsAndLogs);
            configuration.GetSection("Rhino:WorkerConfiguration").Bind(Worker);
        }

        public IConfiguration Configuration { get; }

        public HubConfiguration Hub { get; }

        public ReportConfiguration ReportsAndLogs { get; }

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
            public int MaxCompleted { get; set; }
            public int RepairAttempts { get; set; }
            public double RunningTimeout { get; set; }
        }

        /// <summary>
        /// Child configuration
        /// </summary>
        public class WorkerConfiguration
        {
            public string HubAddress { get; set; }
            public int HubApiVersion { get; set; }
        }

        /// <summary>
        /// Child configuration
        /// </summary>
        public class ReportConfiguration
        {
            public bool Archive { get; set; }
            public string LogsOut { get; set; }
            public IEnumerable<string> Reporters { get; set;}
            public string ReportsOut { get; set; }
        }
    }
}
