using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MongoDB.Automation
{
    public sealed class LocalInstanceProcess<TSettings> : AbstractInstanceProcess<TSettings>
        where TSettings : IInstanceProcessSettings
    {
        private readonly MongoServerAddress _address;
        private readonly Process _process;
        private bool _processIsSupposedToBeRunning;

        public LocalInstanceProcess(string executable, string arguments, TSettings settings)
            : base(settings)
        {
            if (string.IsNullOrEmpty("executable"))
            {
                throw new ArgumentException("Cannot be null or empty.", "executable");
            }

            _address = new MongoServerAddress("localhost", Settings.Port);
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    LoadUserProfile = false,
                    UseShellExecute = false
                }
            };
        }

        public override MongoServerAddress Address
        {
            get { return _address; }
        }

        public override bool IsRunning
        {
            get { return _processIsSupposedToBeRunning && !_process.HasExited; }
        }

        public override void Start()
        {
            if (IsRunning)
            {
                return;
            }

            Console.WriteLine("Starting {0} {1}", _process.StartInfo.FileName, _process.StartInfo.Arguments);

            _processIsSupposedToBeRunning = true;
            _process.Start();
            Util.Timeout(TimeSpan.FromSeconds(20), 
                string.Format("Unable to start instance for address {0}.", Address), 
                TimeSpan.FromSeconds(2),
                remaining =>
                {
                    return IsRunning;
                });
        }

        public override void Stop()
        {
            if (!IsRunning)
            {
                return;
            }
            try
            {
                Console.WriteLine("Stopping instance for address {0}", Address);
                _processIsSupposedToBeRunning = false;
                RunAdminCommand("shutdown");
            }
            catch (EndOfStreamException)
            { } // this is expected
        }
    }
}