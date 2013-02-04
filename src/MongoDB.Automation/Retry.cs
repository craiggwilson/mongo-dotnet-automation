using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MongoDB.Automation
{
    internal static class Retry
    {
        public static void WithTimeout(Func<TimeSpan, bool> action, TimeSpan timeout, TimeSpan delayBetweenAttempts)
        {
            var remaining = timeout;
            Stopwatch watch = new Stopwatch();
            Exception firstException = null;
            do
            {
                watch.Start();
                try
                {
                    if (action(remaining))
                    {
                        return;
                    }
                }
                catch(Exception ex)
                {
                    if (firstException == null)
                    {
                        firstException = ex;
                    }
                }
                finally
                {
                    watch.Stop();
                }
                if (delayBetweenAttempts > TimeSpan.Zero)
                {
                    Thread.Sleep(delayBetweenAttempts);
                }
                remaining = remaining - watch.Elapsed - delayBetweenAttempts;
                watch.Reset();
            }
            while (remaining > TimeSpan.Zero);

            if (firstException != null)
            {
                throw new AutomationException(string.Format("Unable to complete action withing timeout of {0}.", timeout), firstException);
            }
            else
            {
                throw new AutomationException(string.Format("Unable to complete action withing timeout of {0}.", timeout));
            }
        }
    }
}