using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation.Console
{
    public class ConfigurationFactory
    {
        private readonly Dictionary<string, string> _arguments;
        private readonly string _binDirectory;

        public ConfigurationFactory(string binDirectory, IEnumerable<KeyValuePair<string,string>> arguments)
        {
            _arguments = arguments.ToDictionary(x => x.Key, x => x.Value);
            _binDirectory = binDirectory;
        }

        public IControllerConfiguration GetConfiguration()
        {
            if (_arguments.ContainsKey("replSet"))
            {
                return GetReplicaSetConfiguration();
            }

            throw new NotSupportedException();
        }

        private IReplicaSetConfiguration GetReplicaSetConfiguration()
        {
            var replicaSetName = _arguments["replSet"];
            bool useArbiter = _arguments.ContainsKey("arbiterPort");

            int[] ports;
            if (_arguments.ContainsKey("ports"))
            {
                ports = _arguments["ports"].Split(',').Select(x => int.Parse(x)).ToArray();
            }
            else
            {
                ports = new [] { 30000, 30001, 30002 };
            }

            _arguments.Remove("arbiterPort");
            _arguments.Remove("ports");

            var memberConfig = new LocalMongodConfigurationBuilder(_arguments)
                .ExecutablePath(Path.Combine(_binDirectory, "mongod.exe"))
                .Build();

            var replSetBuilder = new LocalReplicaSetConfigurationBuilder()
                .ReplicaSetName(replicaSetName)
                .Ports(ports, memberConfig);

            if(useArbiter)
            {
                replSetBuilder = replSetBuilder.Arbiter(ports[ports.Length - 1], memberConfig);
            }

            return replSetBuilder.Build();
        }
    }
}