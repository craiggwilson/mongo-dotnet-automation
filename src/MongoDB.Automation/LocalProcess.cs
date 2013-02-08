using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public sealed class LocalProcess : AbstractProcess
    {
        private readonly MongoServerAddress _address;
        private readonly Dictionary<string, string> _arguments;
        private readonly string _dbPath;
        private readonly string _logPath;
        private readonly Process _process;
        private bool _processIsSupposedToBeRunning;

        public LocalProcess(string executablePath, IEnumerable<KeyValuePair<string,string>> arguments)
        {
            if (string.IsNullOrEmpty(executablePath))
            {
                throw new ArgumentException("Cannot be null or empty.", "executablePath");
            }

            // store arguments before we resolve dependencies so configuration is transportable.
            _arguments = arguments == null 
                ? new Dictionary<string,string>()
                : arguments.ToDictionary(x => x.Key, x => x.Value);

            var resolved = ResolveCommandArguments(_arguments);

            _dbPath = resolved[Constants.DB_PATH];
            resolved.TryGetValue(Constants.LOG_PATH, out _logPath);

            bool useSysLog = resolved.ContainsKey(Constants.SYS_LOG);

            _address = new MongoServerAddress("localhost", int.Parse(resolved[Constants.PORT]));
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = GetCommandArguments(resolved),
                    CreateNoWindow = !string.IsNullOrEmpty(_logPath) || useSysLog,
                    WindowStyle = string.IsNullOrEmpty(_logPath) && !useSysLog 
                        ? ProcessWindowStyle.Normal
                        : ProcessWindowStyle.Hidden
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

        public override IProcessConfiguration GetConfiguration()
        {
            return new LocalProcessConfiguration(_process.StartInfo.FileName, _arguments);
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
                // let's make sure we waited long enough for the process to die...
                Thread.Sleep(TimeSpan.FromSeconds(2)); 
                Retry.WithTimeout(
                    _ => IsRunning,
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(2));
            }
            catch(Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("Unable to start instance for address {0}.", Address).AppendLine();
                sb.AppendFormat("Command Line: {0}", Arguments).AppendLine();
                throw new AutomationException(sb.ToString(), ex);
            }

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
            Directory.CreateDirectory(_dbPath);
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
            Directory.Delete(_dbPath, true);
        }

        private void RemoveLogPath(StartOptions options)
        {
            if (!string.IsNullOrEmpty(_logPath))
            {
                var exists = File.Exists(_logPath);
                if (exists && options == StartOptions.Clean)
                {
                    Config.Out.WriteLine("Removing file at {0}", _logPath);
                    File.Delete(_logPath);
                }
            }
        }

        private static Dictionary<string, string> ResolveCommandArguments(Dictionary<string,string> arguments)
        {
            if (!arguments.ContainsKey(Constants.PORT))
            {
                arguments.Add(Constants.PORT, Config.DefaultPort.ToString());
            }
            if (!arguments.ContainsKey(Constants.DB_PATH))
            {
                arguments.Add(Constants.DB_PATH, Config.DefaultDbPath);
            }

            // This is a simple topological sort.

            var remaining = arguments.Select(x => new CommandLineArgument(x.Key, x.Value)).ToList();
            var result = new Dictionary<string, string>();
            var set = new Queue<CommandLineArgument>(remaining.Where(x => x.Dependencies.Count == 0));
            
            remaining = remaining.Where(x => x.Dependencies.Count > 0).ToList();
            if (set.Count == 0)
            {
                throw new AutomationException("Every argument depends on another argument. At least one argument must exist that has no dependencies.");
            }

            while (set.Count > 0)
            {
                var current = set.Dequeue();
                result.Add(current.Name, current.Value);

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

            return result;
        }

        private static string GetCommandArguments(IEnumerable<KeyValuePair<string,string>> arguments)
        {
            List<string> args = new List<string>();
            foreach (var arg in arguments)
            {
                string actual = "--" + arg.Key;
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
                Value = Value.Replace("{" + name + "}", value ?? name);
                Dependencies = GetDependencies(Value);
            }

            private static List<string> GetDependencies(string value)
            {
                if (value == null)
                {
                    return new List<string>();
                }
                var match = _dependenciesRegex.Match(value);
                var group = match.Groups["KeyName"];
                return group.Captures.OfType<Capture>().Select(x => x.Value).ToList();
            }
        }
    }
}