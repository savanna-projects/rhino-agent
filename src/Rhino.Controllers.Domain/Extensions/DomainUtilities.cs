using Rhino.Controllers.Domain.Cache;

using System.Diagnostics;

namespace Rhino.Controllers.Domain.Extensions
{
    public static class DomainUtilities
    {
        public static void SyncCache()
        {
            static void GetAnimation(int animationSpeed, CancellationToken token)
            {
                // setup
                var loaderChars = new char[] { '|', '/', '-', '\\' };

                // Loop through the loader characters and render them to the console
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        foreach (char c in loaderChars)
                        {
                            Console.Write(c);
                            Thread.Sleep(animationSpeed);
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }
                            try
                            {
                                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                            }
                            catch (Exception e) when (e != null)
                            {
                                // ignore
                            }
                        }
                    }
                }, token);
            }

            try
            {
                Console.Write("Loading Application Cache, Please Wait ");
                var tokenSource = new CancellationTokenSource();
                var stopwatch = new Stopwatch();

                stopwatch.Start();
                GetAnimation(animationSpeed: 75, token: tokenSource.Token);

                var plugins = MetaDataCache.Plugins.Sum(i => i.Value.ActionsCache.Count);
                stopwatch.Stop();
                tokenSource.Cancel();
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                Console.Write("... Done!");

                Console.WriteLine();
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
