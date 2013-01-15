using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Local
{
    public class LocalReplicaSetBuilder
    {
        private LocalProcessTemplate _processTemplate;
        private string _replicaSetName;
        private IEnumerable<int> _ports;

        public LocalReplicaSetBuilder BuildMembersWith(Action<ILocalReplicaSetMemberBuilder> memberBuilder)
        {
            _processTemplate = new LocalProcessTemplate
            {
                Factories = new Dictionary<string, Func<LocalReplicaSetMember, string>>(),
                Values = new Dictionary<string,string>()
            };

            var builder = new LocalReplicaSetMemberBuilder(_processTemplate.Factories, _processTemplate.Values);
            memberBuilder(builder);
            return this;
        }

        public LocalReplicaSetBuilder Ports(IEnumerable<int> ports)
        {
            _ports = ports;
            return this;
        }

        public LocalReplicaSetBuilder Ports(params int[] ports)
        {
            return Ports((IEnumerable<int>)ports);
        }

        public LocalReplicaSetBuilder ReplicaSetName(string name)
        {
            _replicaSetName = name;
            return this;
        }

        public ReplicaSetController Build()
        {
            List<IInstanceProcess> processes = new List<IInstanceProcess>();

            foreach (var port in _ports)
            {
                var values = _processTemplate.Values.ToDictionary(x => x.Key, x => x.Value); // make a copy
                values["port"] = port.ToString();
                var member = new LocalReplicaSetMember(port, _replicaSetName ?? "test");
                foreach (var factory in _processTemplate.Factories)
                {
                    values[factory.Key] = factory.Value(member);
                }

                string binPath;
                if(!values.TryGetValue("BIN_PATH", out binPath))
                {
                    throw new AutomationException("A bin path is required to be specified.");
                }
                values.Remove("BIN_PATH");

                processes.Add(new LocalInstanceProcess(binPath, values));
            }

            return new ReplicaSetController(_replicaSetName ?? "test", null);
        }

        private class LocalProcessTemplate
        {
            public Dictionary<string, Func<LocalReplicaSetMember, string>> Factories;
            public Dictionary<string, string> Values;
        }

        private class LocalReplicaSetMemberBuilder : ILocalReplicaSetMemberBuilder
        {
            private readonly Dictionary<string, Func<LocalReplicaSetMember, string>> _factories;
            private readonly Dictionary<string, string> _values;

            public LocalReplicaSetMemberBuilder(Dictionary<string, Func<LocalReplicaSetMember, string>> factories, Dictionary<string, string> values)
            {
                _factories = factories;
                _values = values;
            }

            public ILocalReplicaSetMemberBuilder BinPath(string binPath)
            {
                _values["BIN_PATH"] = binPath;
                return this;
            }

            public ILocalReplicaSetMemberBuilder DbPath(Func<LocalReplicaSetMember, string> dbPathFactory)
            {
                _factories["dbpath"] = dbPathFactory;
                return this;
            }

            public ILocalReplicaSetMemberBuilder LogPath(string logPath)
            {
                return Set("logpath", logPath);
            }

            public ILocalReplicaSetMemberBuilder LogPath(Func<LocalReplicaSetMember, string> logPathFactory)
            {
                _factories["logpath"] = logPathFactory;
                return this;
            }

            public ILocalReplicaSetMemberBuilder Ipv6()
            {
                return Set("ipv6", null);
            }

            public ILocalReplicaSetMemberBuilder NoJournal()
            {
                return Set("nojournal", "true");
            }

            public ILocalReplicaSetMemberBuilder OplogSize(int size)
            {
                return Set("oplogsize", size.ToString());
            }

            public ILocalReplicaSetMemberBuilder Set(string name, string value)
            {
                _values[name] = value;
                return this;
            }
        }
    }
}