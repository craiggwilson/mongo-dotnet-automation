using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Local
{
    public class LocalReplicaSetBuilder
    {
        private int? _arbiterPort;
        private string _setName;
        private Dictionary<int, LocalReplicaSetMongodBuilder> _templates;

        public LocalReplicaSetBuilder()
        {
            _templates = new Dictionary<int, LocalReplicaSetMongodBuilder>();
        }

        public ReplicaSetController Build()
        {
            if (string.IsNullOrEmpty(_setName))
            {
                _setName = Config.DefaultReplicaSetName;
            }

            var processes = _templates
                .Select(x => x.Value
                    .Port(x.Key)
                    .SetName(_setName)
                    .Build())
                .OfType<IInstanceProcess>();

            return new ReplicaSetController(_setName, processes);
        }

        public LocalReplicaSetBuilder Arbiter(int port, LocalReplicaSetMongodBuilder template)
        {
            _arbiterPort = port;
            return Port(port, template);
        }

        public LocalReplicaSetBuilder Port(int port, LocalReplicaSetMongodBuilder template)
        {
            return Ports(new [] { port }, template);
        }

        public LocalReplicaSetBuilder Port(int port1, int port2, LocalReplicaSetMongodBuilder template)
        {
            return Ports(new [] { port1, port2 }, template);
        }

        public LocalReplicaSetBuilder Port(int port1, int port2, int port3, LocalReplicaSetMongodBuilder template)
        {
            return Ports(new [] { port1, port2, port3 }, template);
        }

        public LocalReplicaSetBuilder Ports(IEnumerable<int> ports, LocalReplicaSetMongodBuilder template)
        {
            foreach (var port in ports)
            {
                _templates[port] = template;
            }

            return this;
        }

        public LocalReplicaSetBuilder ReplicaSetName(string name)
        {
            _setName = name;
            return this;
        }
    }
}