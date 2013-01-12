using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MongoDB.Automation
{
    internal static class Util
    {
        public static void Timeout(TimeSpan timeout, string timeoutMessage, TimeSpan delay, Func<TimeSpan, bool> attempt)
        {
            Stopwatch watch = new Stopwatch();
            do
            {
                watch.Start();
                try
                {
                    if (attempt(timeout))
                    {
                        return;
                    }
                }
                catch
                { }
                finally
                {
                    watch.Stop();
                }
                if (delay > TimeSpan.Zero)
                {
                    Thread.Sleep(delay);
                }
                timeout = timeout - watch.Elapsed - delay;
                watch.Reset();
            }
            while (timeout > TimeSpan.Zero);

            Config.Error.WriteLine(timeoutMessage);
            throw new AutomationException(timeoutMessage);
        }
    }
}