using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

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
            if (string.IsNullOrEmpty(executable))
            {
                throw new ArgumentException("Cannot be null or empty.", "executable");
            }

            var args = arguments == null
                ? new Dictionary<string, string>()
                : arguments.ToDictionary(x => x.Key, x => x.Value);

            string port;
            if (!args.TryGetValue("port", out port))
            {
                port = Config.DefaultPort.ToString();
                args.Add("port", port);
            }

            if (!args.TryGetValue("dbpath", out _dbPath))
            {
                _dbPath = Config.DefaultDbPath;
                args.Add("dbpath", _dbPath);
            }

            args.TryGetValue("logpath", out _logPath);

            _address = new MongoServerAddress("localhost", int.Parse(port));
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = GetCommandArguments(args),
                    CreateNoWindow = true,
                    LoadUserProfile = false,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
        }

        public override MongoServerAddress Address
        {
            get { return _address; }
        }

        public string Arguments
        {
            get { return _process.StartInfo.Arguments; }
        }

        public override bool IsRunning
        {
            get { return _processIsSupposedToBeRunning && !_process.HasExited; }
        }

        public string ReadOutput()
        {
            return _process.StandardOutput.ReadToEnd();
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
            Config.Out.WriteLine("Creating directory at {0}", _dbPath);
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

        private static string GetCommandArguments(IEnumerable<KeyValuePair<string,string>> arguments)
        {
            // This is a simple topological sort.

            var remaining = arguments.Select(x => new CommandLineArgument(x.Key, x.Value)).ToList();
            var list = new List<CommandLineArgument>();
            var set = new Queue<CommandLineArgument>(remaining.Where(x => x.Dependencies.Count == 0));
            
            remaining = remaining.Where(x => x.Dependencies.Count > 0).ToList();
            if (set.Count == 0)
            {
                throw new AutomationException("Every argument depends on another argument. At least one argument must exist that has no dependencies.");
            }

            while (set.Count > 0)
            {
                var current = set.Dequeue();
                list.Add(current);

                foreach (var node in remaining.Where(x => x.Dependencies.Contains(current.Name)).ToList())
                {
                    node.ReplaceDependency(current.Name, current.Value);

                    if (node.Dependencies.Count == 0)
                    {
                        set.Enqueue(node);
                        remaining.Remove(node);
                    }
                }
            }

            if (remaining.Count != 0)
            {
                // TODO: explain which nodes had problems...
                throw new AutomationException("A cycle exists where two arguments depend on each other. Remove the cycle and try again.");
            }

            List<string> args = new List<string>();
            foreach (var arg in list)
            {
                string actual = "--" + arg.Name;
                if (arg.Value != null)
                {
                    actual += " " + arg.Value;
                }
                args.Add(actual);
            }

            return string.Join(" ", args.ToArray());
        }

        private class CommandLineArgument
        {
            private static readonly Regex _dependenciesRegex = new Regex(@"^(.*?(\{(?<KeyName>[a-zA-Z0-9]+)})?)*$",
                RegexOptions.Compiled | RegexOptions.Singleline);

            public string Name;
            public string Value;
            public List<string> Dependencies;

            public CommandLineArgument(string name, string value)
            {
                Name = name;
                Value = value;
                Dependencies = GetDependencies(value);
            }

            public void ReplaceDependency(string name, string value)
            {
                Value = Value.Replace("{" + name + "}", value);
                Dependencies = GetDependencies(Value);
            }

            private static List<string> GetDependencies(string value)
            {
                var match = _dependenciesRegex.Match(value);
                var group = match.Groups["KeyName"];
                return group.Captures.OfType<Capture>().Select(x => x.Value).ToList();
            }
        }
    }
}