/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rhino.Api.Contracts.AutomationProvider;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhino.Controllers.Extensions
{
    public static class ControllerUtilities
    {
        // constants
        public const string LogsConfigurationKey = "Rhino:ReportConfiguration:LogsOut";
        public const string ReportsConfigurationKey = "Rhino:ReportConfiguration:ReportsOut";

        /// <summary>
        /// Gets the default logs output folder path (not including the log name);
        /// </summary>
        public static string LogsDefaultFolder => GetLogsDefaultFolder();

        /// <summary>
        /// Normalize driver parameters to match Gravity's driver parameters contract.
        /// </summary>
        /// <param name="driverParameters">Driver parameters to normalize.</param>
        /// <returns>Normalized driver parameters.</returns>
        public static IEnumerable<IDictionary<string, object>> ParseDriverParameters(IEnumerable<IDictionary<string, object>> driverParameters)
        {
            // setup
            var onDriverParameters = new List<IDictionary<string, object>>();

            // iterate
            foreach (var item in driverParameters)
            {
                var driverParam = item;
                if (driverParam.ContainsKey(ContextEntry.Capabilities))
                {
                    var capabilitiesBody = ((JObject)driverParam[ContextEntry.Capabilities]).ToString();
                    driverParam[ContextEntry.Capabilities] =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(capabilitiesBody);
                }
                onDriverParameters.Add(driverParam);
            }

            // results
            return onDriverParameters;
        }

        #region *** Logger  ***
        /// <summary>
        /// Gets a new instance of a default ILogger with TraceLogger implementation.
        /// </summary>
        /// <param name="configuration">Configuration by which to created log files and log level (optional).</param>
        /// <returns>An ILogger instance.</returns>
        public static ILogger GetLogger(IConfiguration configuration)
        {
            return DoGetLogger(configuration);
        }

        /// <summary>
        /// Gets a new instance of a default ILogger with TraceLogger implementation.
        /// </summary>
        /// <param name="type">The logger type.</param>
        /// <returns>An ILogger instance.</returns>
        public static ILogger GetLogger(Type type)
        {
            // constants
            const string File = "appsettings.json";
            var name = type.Name;
            var root = Environment.CurrentDirectory;

            // setup
            var configuration = new ConfigurationBuilder().SetBasePath(root).AddJsonFile(File).Build();

            // build
            return DoGetLogger(configuration).CreateChildLogger(name);
        }

        private static ILogger DoGetLogger(IConfiguration configuration)
        {
            // get in folder
            var inFolder = configuration.GetValue<string>(LogsConfigurationKey);
            inFolder = string.IsNullOrEmpty(inFolder) ? GetLogsDefaultFolder() : inFolder;

            // setup logger
            return new TraceLogger(applicationName: "RhinoApi", loggerName: string.Empty, inFolder);
        }
        #endregion

        #region *** Reports ***
        /// <summary>
        /// Gets a list of available reports created by automation runs.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> by which to fetch settings.</param>
        /// <returns>A list of reports.</returns>
        public static IEnumerable<(string Path, string Name)> GetReports(IConfiguration configuration)
        {
            // setup
            var path = DoGetStaticReportsFolder(configuration);

            // get
            return Directory.GetDirectories(path).Select(i => (i, Path.GetFileName(i)));
        }

        /// <summary>
        /// Gets the static reports folder in which static reports can be served.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> by which to fetch settings.</param>
        /// <returns>Static reports folder.</returns>
        public static string GetStaticReportsFolder(IConfiguration configuration)
        {
            return DoGetStaticReportsFolder(configuration);
        }

        private static string DoGetStaticReportsFolder(IConfiguration configuration)
        {
            // setup
            var onFolder = configuration.GetValue(ReportsConfigurationKey, ".");

            // is current location
            if (onFolder == ".")
            {
                onFolder = Path.Join(Environment.CurrentDirectory, "Outputs", "Reports", "rhino");
            }
            onFolder = onFolder.Replace(Path.GetFileName(onFolder), string.Empty);

            // setup
            return Path.IsPathRooted(onFolder) ? onFolder : Path.Join(Environment.CurrentDirectory, onFolder);
        }
        #endregion

        /// <summary>
        /// Force a file reading even if the file is open by another process.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A string containing all the text in the file.</returns>
        public static Task<string> ForceReadFileAsync(string path)
        {
            return InvokeForceReadFileAsync(path);
        }

        /// <summary>
        /// Gets the default JsonSerializerSettings (prettify and camelCase)
        /// </summary>
        public static JsonSerializerOptions JsonSettings => new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        /// <summary>
        /// Gets the version the Rhino Server instance (if available).
        /// </summary>
        /// <returns>Rhino Server version.</returns>
        public static Task<string> GetVersionAsync()
        {
            return InvokeGetVersionAsync();
        }

        #region *** Graphics   ***
        /// <summary>
        /// Renders RhinoAPI logo in the console.
        /// </summary>
        public static void RenderApiLogo()
        {
            try
            {
                Console.Clear();

                DoRenderLogo(1, 1, Console.BackgroundColor, Console.ForegroundColor, Rhino());
                DoRenderLogo(1, 32, Console.BackgroundColor, ConsoleColor.Red, Api());

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(new string(' ', 24) + "Powered by Gravity Engine");
                Console.WriteLine(new string(' ', 24) + "Version 0.0.0.0");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("https://github.com/savanna-projects/rhino-agent");
                Console.WriteLine("https://github.com/gravity-api");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (Exception e) when (e != null)
            {
                // ignore errors
            }
        }

        /// <summary>
        /// Renders RhinoWorker logo in the console.
        /// </summary>
        public static void RenderWorkerLogo()
        {
            try
            {
                Console.Clear();

                DoRenderLogo(1, 1, Console.BackgroundColor, Console.ForegroundColor, Rhino());
                DoRenderLogo(1, 31, Console.BackgroundColor, ConsoleColor.Red, Worker());

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(new string(' ', 48) + "Powered by Gravity Engine");
                Console.WriteLine(new string(' ', 48) + "Version 0.0.0.0");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("https://github.com/savanna-projects/rhino-agent");
                Console.WriteLine("https://github.com/gravity-api");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (Exception e) when (e != null)
            {
                // ignore errors
            }
        }

        private static void DoRenderLogo(
            int startRow,
            int startColumn,
            ConsoleColor background,
            ConsoleColor foreground,
            IEnumerable<string> lines)
        {
            // setup
            Console.CursorTop = startRow;
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;

            // render
            for (int i = 0; i < lines.Count(); i++)
            {
                Console.CursorTop = startRow + i;
                Console.CursorLeft = startColumn;
                Console.WriteLine(lines.ElementAt(i));
            }
        }

        private static IEnumerable<string> Rhino() => new List<string>
        {
            "▄▄▄▄▄▄ ██     █▄              ",
            "██  ██ ██▄▄▄  ▄▄ ▄▄▄▄▄   ▄▄▄▄ ",
            "█████▀ ██ ▀██ ██ ██▀▀██ ██ ▀██",
            "██  ██ ██  ██ ██ ██  ██ ██ ▄██",
            "▀▀  ▀▀ ▀▀  ▀▀ ▀▀ ▀▀  ▀▀  ▀▀▀▀ ",
        };

        private static IEnumerable<string> Api() => new List<string>
        {
            "  ███   ██▀██▄ ██",
            " ██ ██  ██ ▄█▀ ██",
            "▄█████▄ ██▀▀▀  ██",
            "██   ██ ██     ██",
            "▀▀   ▀▀ ▀▀     ▀▀",
        };

        private static IEnumerable<string> Worker() => new List<string>
        {
            "▄▄▄  ▄▄  ▄▄             ██                ",
            " ██ ███ ▄█▀  ▄▄▄▄  ▄▄▄▄ ██ ▄▄   ▄▄▄▄  ▄▄▄▄",
            " ██▄███▄██  ██ ▀██ ███▀ ████   ██▄▄██ ███▀",
            "  ███ ███▀  ██ ▄██ ██   ██▀█▄  ███▀▀  ██  ",
            "  ▀▀▀  ▀▀    ▀▀▀▀  ▀▀   ▀▀ ▀▀▀  ▀▀▀▀  ▀▀  ",
        };
        #endregion

        // Utilities
        private static string GetLogsDefaultFolder() => Path.Join(Environment.CurrentDirectory, "Logs");

        public static async Task<string> InvokeGetVersionAsync()
        {
            // constants
            const string FileName = "version.txt";
            const string Default = "0.0.0.0";

            // not found
            if (!File.Exists(FileName))
            {
                return Default;
            }

            // extract
            var version = (await InvokeForceReadFileAsync(path: FileName).ConfigureAwait(false)).Trim();

            // get
            return string.IsNullOrEmpty(version) ? Default : version;
        }

        private static async Task<string> InvokeForceReadFileAsync(string path)
        {
            // force open
            var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // build
            var reader = new StreamReader(stream);

            // read
            var file = await reader.ReadToEndAsync().ConfigureAwait(false);

            // cleanup
            reader.Dispose();
            await stream.DisposeAsync().ConfigureAwait(false);

            // get
            return file;
        }
    }
}
