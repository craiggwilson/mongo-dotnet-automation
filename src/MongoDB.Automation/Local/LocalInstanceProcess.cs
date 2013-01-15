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
    public sealed class LocalInstanceProcess : AbstractInstanceProcess
    {
        private readonly MongoServerAddress _address;
        private readonly string _dbPath;
        private readonly string _logPath;
        private readonly Process _process;
        private bool _processIsSupposedToBeRunning;

        public LocalInstanceProcess(string executable, IDictionary<string, string> arguments)
        {
            if (string.IsNullOrEmpty("executable"))
            {
                throw new ArgumentException("Cannot be null or empty.", "executable");
            }

            string port;
            if (!arguments.TryGetValue("port", out port))
            {
                port = "27017"; // this is the default...
            }

            _address = new MongoServerAddress("localhost", int.Parse(port));
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = GetCommandArguments(arguments),
                    CreateNoWindow = true,
                    LoadUserProfile = false,
                    UseShellExecute = false
                }
            };

            if (!arguments.TryGetValue("dbpath", out _dbPath))
            {
                _dbPath = "C:\\data\\db";
                if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    _dbPath = "/data/db";
                }
            }

            arguments.TryGetValue("logpath", out _logPath);
        }

        public override MongoServerAddress Address
        {
            get { return _address; }
        }

        public override bool IsRunning
        {
            get { return _processIsSupposedToBeRunning && !_process.HasExited; }
        }

        public override void Start(StartOptions options)
        {
            if (IsRunning)
            {
                return;
            }

            Config.Out.WriteLine("Starting {0} {1}.", _process.StartInfo.FileName, _process.StartInfo.Arguments);

            if (!string.IsNullOrEmpty(_dbPath))
            {
                var exists = Directory.Exists(_dbPath);
                if (exists && options == StartOptions.Clean)
                {
                    RemoveDbPath();
                }
                if (!exists)
                {
                    CreateDbPath();
                }
            }

            if (!string.IsNullOrEmpty(_logPath))
            {
                var exists = File.Exists(_logPath);
                if (exists && options == StartOptions.Clean)
                {
                    RemoveLogPath();
                }
            }

            Config.Out.WriteLine("Starting {0} {1}.", _process.StartInfo.FileName, _process.StartInfo.Arguments);

            _processIsSupposedToBeRunning = true;
            _process.Start();
            Util.Timeout(TimeSpan.FromSeconds(20), 
                string.Format("Unable to start instance for address {0}.", Address), 
                TimeSpan.FromSeconds(2),
                remaining =>
                {
                    return IsRunning;
                });
            Config.Out.WriteLine("Process {0} started.", _process.Id);
        }

        public override void Stop()
        {
            if (!IsRunning)
            {
                return;
            }
            try
            {
                Config.Out.WriteLine("Stopping instance for address {0}.", Address);
                _processIsSupposedToBeRunning = false;
                RunAdminCommand("shutdown");
            }
            catch (EndOfStreamException)
            { } // this is expected for shutdown
            finally
            {
                Thread.Sleep(TimeSpan.FromSeconds(4));
                if (!_process.HasExited)
                {
                    try
                    {
                        _process.Kill();
                    }
                    catch { }
                }
                Config.Out.WriteLine("Process {0} stopped.", _process.Id);
            }
        }

        private void CreateDbPath()
        {
            try
            {
                Directory.CreateDirectory(_dbPath);
            }
            catch (Exception ex)
            {
                Config.Error.WriteLine("Unable to create directory: {0}", ex.Message);
                throw;
            }
        }

        protected string GetCommandArguments(IDictionary<string, string> arguments)
        {
            List<string> args = new List<string>();
            foreach (var pair in arguments)
            {
                string arg = "--" + pair.Key;
                if (pair.Value != null)
                {
                    arg += " " + pair.Value;
                }
                args.Add(arg);
            }

            return string.Join(" ", args.ToArray());
        }

        private void RemoveDbPath()
        {
            Config.Out.WriteLine("Removing directory at {0}", _dbPath);
            try
            {
                Directory.Delete(_dbPath, true);
            }
            catch (Exception ex)
            {
                Config.Error.WriteLine("Unable to remove directory: {0}", ex.Message);
                throw;
            }
        }

        private void RemoveLogPath()
        {
            Config.Out.WriteLine("Removing file at {0}", _logPath);
            try
            {
                File.Delete(_logPath);
            }
            catch (Exception ex)
            {
                Config.Error.WriteLine("Unable to remove file: {0}", ex.Message);
                throw;
            }
        }
    }
}