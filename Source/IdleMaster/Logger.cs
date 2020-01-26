using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace IdleMaster
{
    public static class Logger
    {
        private static readonly object LogLock = new object();

        private static readonly string ExceptionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\error.log";

        public static void Exception(Exception ex, params string[] messages)
        {
            var contents = string.Concat(DateTime.Now, "   ", string.Join(Environment.NewLine, messages),
              Environment.NewLine, ex.ToString(), Environment.NewLine);
            Write(contents, ExceptionPath);
        }

        private static void Write(string contents, string path)
        {
            Console.WriteLine(contents);
            using (TimedLock.Lock(LogLock))
            {
                File.AppendAllText(path, contents, Encoding.UTF8);
            }
        }
    }


    // Thanks to Eric Gunnerson for recommending this be a struct rather
    // than a class - avoids a heap allocation.
    // Thanks to Change Gillespie and Jocelyn Coulmance for pointing out
    // the bugs that then crept in when I changed it to use struct...
    // Thanks to John Sands for providing the necessary incentive to make
    // me invent a way of using a struct in both release and debug builds
    // without losing the debug leak tracking.

    public struct TimedLock : IDisposable
    {
        public static TimedLock Lock(object o)
        {
            return Lock(o, TimeSpan.FromSeconds(10));
        }

        public static TimedLock Lock(object o, TimeSpan timeout)
        {
            var tl = new TimedLock(o);
            if (!Monitor.TryEnter(o, timeout))
            {
#if DEBUG
                GC.SuppressFinalize(tl.leakDetector);
#endif
                throw new LockTimeoutException();
            }

            return tl;
        }

        private TimedLock(object o)
        {
            target = o;
#if DEBUG
            leakDetector = new Sentinel();
#endif
        }
        private object target;

        public void Dispose()
        {
            Monitor.Exit(target);

            // It's a bad error if someone forgets to call Dispose,
            // so in Debug builds, we put a finalizer in to detect
            // the error. If Dispose is called, we suppress the
            // finalizer.
#if DEBUG
            GC.SuppressFinalize(leakDetector);
#endif
        }

#if DEBUG
        // (In Debug mode, we make it a class so that we can add a finalizer
        // in order to detect when the object is not freed.)
        private class Sentinel
        {
            ~Sentinel()
            {
                // If this finalizer runs, someone somewhere failed to
                // call Dispose, which means we've failed to leave
                // a monitor!
                Debug.Fail("Undisposed lock");
            }
        }
        private Sentinel leakDetector;
#endif

    }
    public class LockTimeoutException : ApplicationException
    {
        public LockTimeoutException()
          : base("Timeout waiting for lock")
        {
        }
    }
}