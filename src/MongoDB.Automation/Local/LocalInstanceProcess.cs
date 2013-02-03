using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MongoDB.Automation.Local
{
    public sealed class LocalInstanceProcess : AbstractInstanceProcess
    {
        private readonly MongoServerAddress _address;
        private readonly string _dbPath;
        private readonly string _logPath;
        private readonly Process _process;
        private bool _processIsSupposedToBeRunning;

        public LocalInstanceProcess(string executable)
            : this(executable, new Dictionary<string, string>())
        { }

        public LocalInstanceProcess(string executable, IEnumerable<KeyValuePair<string, string>> arguments)
        {
            if (string.IsNullOrEmpty("executable") || !File.Exists(executable))
            {
                throw new ArgumentException("Cannot be null or empty.", "executable");
            }

            var args = arguments == null
                ? new Dictionary<string, string>()
                : arguments.ToDictionary(x => x.Key, x => x.Value);

            string port;
            if (!args.TryGetValue("port", out port))
            {
                port = "27017"; // this is the default...
            }

            _address = new MongoServerAddress("localhost", int.Parse(port));
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = GetCommandArguments(args),
                    CreateNoWindow = true,
                    LoadUserProfile = false,
                    UseShellExecute = false
                }
            };

            if (!args.TryGetValue("dbpath", out _dbPath))
            {
                _dbPath = "C:\\data\\db";
                if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    _dbPath = "/data/db";
                }
            }

            args.TryGetValue("logpath", out _logPath);
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

            EnsureDbPath(options);
            RemoveLogPath(options);
            
            _processIsSupposedToBeRunning = true;

            try
            {
                _process.Start();
            }
            catch(Exception ex)
            {
                Config.Error.WriteLine("Unable to start instance for address {0}. {1}", Address, ex);
                throw;
            }

            Util.Timeout(TimeSpan.FromSeconds(20), 
                string.Format("Unable to start instance for address {0}.", Address), 
                TimeSpan.FromSeconds(2),
                remaining => IsRunning);

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
                RunAdminCommand("shutdown");
            }
            catch (EndOfStreamException)
            { } // this is expected for shutdown
            finally
            {
                _processIsSupposedToBeRunning = false;
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

        private void EnsureDbPath(StartOptions options)
        {
            var dbPathExists = Directory.Exists(_dbPath);
            if (dbPathExists && options == StartOptions.Clean)
            {
                RemoveDbPath();
                dbPathExists = false;
            }

            if (!dbPathExists)
            {
                CreateDbPath();
            }
        }

        private string GetCommandArguments(IDictionary<string, string> arguments)
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


        private void RemoveLogPath(StartOptions options)
        {
            if (!string.IsNullOrEmpty(_logPath))
            {
                var exists = File.Exists(_logPath);
                if (exists && options == StartOptions.Clean)
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
    }
}