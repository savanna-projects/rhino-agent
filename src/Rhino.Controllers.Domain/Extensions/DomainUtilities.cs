using Rhino.Controllers.Domain.Cache;

using System.Diagnostics;

namespace Rhino.Controllers.Domain.Extensions
{
    public static class DomainUtilities
    {
        public static void SyncCache()
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                Console.WriteLine("Loading Application Cache, Please Wait...");

                var plugins = MetaDataCache.Plugins.SelectMany(i => i.Value.ActionsCache).Count();
                stopwatch.Stop();

                Console.WriteLine($"Total of {plugins} Entities Cached; Time (sec.): {stopwatch.ElapsedMilliseconds / 1000}");
                Console.WriteLine();
            }
            catch (Exception e) when (e != null)
            {
                Trace.TraceError($"{e}");
                Console.WriteLine($"Sync-Plugins = (InternalServerError | {e.Message})");
            }
        }
    }
}
