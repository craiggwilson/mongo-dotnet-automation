using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public static class Config
    {
        private static readonly object _writerLock = new object();
        private static Func<TextWriter> _error;
        private static Func<TextWriter> _out;

        static Config()
        {
            _error = () => Console.Error;
            _out = () => Console.Out;
        }

        /// <summary>
        /// Gets the default db path.
        /// </summary>
        public static string DefaultDbPath
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    return "/data/db";
                }

                return "c:\\data\\db";
            }
        }

        /// <summary>
        /// Gets the default port.
        /// </summary>
        public static int DefaultPort
        {
            get { return 27017; }
        }

        public static string DefaultReplicaSetName
        {
            get { return "rs0"; }
        }

        /// <summary>
        /// Gets the TextWriter to send errors.
        /// </summary>
        public static TextWriter Error
        {
            get
            {
                lock (_writerLock)
                {
                    return _error();
                }
            }
        }

        /// <summary>
        /// Gets the TextWriter to send information.
        /// </summary>
        public static TextWriter Out
        {
            get 
            {
                lock (_writerLock)
                {
                    return _out();
                }
            }
        }

        /// <summary>
        /// Sets the TextWriter to send errors.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <exception cref="System.ArgumentNullException">writer</exception>
        public static void SetError(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            lock (_writerLock)
            {
                _error = () => writer;
            }
        }

        /// <summary>
        /// Sets the TextWriter to send information.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <exception cref="System.ArgumentNullException">writer</exception>
        public static void SetOut(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            lock (_writerLock)
            {
                _out = () => writer;
            }
        }
    }
}