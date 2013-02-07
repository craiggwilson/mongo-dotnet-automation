using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class LocalReplicaSetConfigurationBuilder
    {
        private int? _arbiterPort;
        private string _setName;
        private Dictionary<int, ILocalProcessConfiguration> _templates;

        public LocalReplicaSetConfigurationBuilder()
        {
            _templates = new Dictionary<int, ILocalProcessConfiguration>();
        }

        public IReplicaSetConfiguration Build()
        {
            if (string.IsNullOrEmpty(_setName))
            {
                _setName = Config.DefaultReplicaSetName;
            }

            var processes = _templates
                .Select(t => new LocalReplicaSetMongodConfigurationBuilder(t.Value)
                    .Port(t.Key)
                    .ReplSet(_setName)
                    .Build())
                .OfType<IProcessConfiguration>();

            return new ReplicaSetConfiguration(_setName, processes, _arbiterPort);
        }

        public LocalReplicaSetConfigurationBuilder Arbiter(int port, ILocalProcessConfiguration configuration)
        {
            _arbiterPort = port;
            return Port(port, configuration);
        }

        public LocalReplicaSetConfigurationBuilder Port(int port, ILocalProcessConfiguration configuration)
        {
            return Ports(new [] { port }, configuration);
        }

        public LocalReplicaSetConfigurationBuilder Port(int port1, int port2, ILocalProcessConfiguration configuration)
        {
            return Ports(new [] { port1, port2 }, configuration);
        }

        public LocalReplicaSetConfigurationBuilder Port(int port1, int port2, int port3, ILocalProcessConfiguration configuration)
        {
            return Ports(new [] { port1, port2, port3 }, configuration);
        }

        public LocalReplicaSetConfigurationBuilder Ports(IEnumerable<int> ports, ILocalProcessConfiguration configuration)
        {
            foreach (var port in ports)
            {
                _templates[port] = configuration;
            }

            return this;
        }

        public LocalReplicaSetConfigurationBuilder ReplicaSetName(string name)
        {
            _setName = name;
            return this;
        }
    }
}