using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation.Console
{
    public class StartCommand
    {
        private readonly Dictionary<string, string> _args;
        private readonly string _binDirectory;
        private readonly string _verb;

        public StartCommand(string verb, string binDirectory, Dictionary<string, string> args)
        {
            _verb = verb;
            _args = args;
            _binDirectory = binDirectory;
        }

        public void Run()
        {
            IControllerConfiguration config;
            if (_args.ContainsKey("replSet"))
            {
                config = GetReplicaSetConfiguration();
            }
            else
            {
                config = GetStandAloneConfiguration();
            }

            try
            {
                new Automate()
                    .From(config)
                    .Start(_verb == "restart" ? StartOptions.None : StartOptions.Clean);
            }
            catch (AutomationException ex)
            {
                System.Console.WriteLine("Unable to {0} using the supplied parameters.", _verb);
                System.Console.WriteLine(ex.Message);
            }
        }

        private StandAloneConfiguration GetStandAloneConfiguration()
        {
            int port = 27017;
            if (_args.ContainsKey("port"))
            {
                port = int.Parse(_args["port"]);
            }

            var serverConfig = new LocalMongodConfigurationBuilder(_args)
                .ExecutablePath(Path.Combine(_binDirectory, "mongod.exe"))
                .Build();

            return new StandAloneConfiguration(serverConfig);
        }

        private ReplicaSetConfiguration GetReplicaSetConfiguration()
        {
            var replicaSetName = _args["replSet"];
            bool useArbiter = _args.ContainsKey("useArbiter");

            int[] ports;
            if (_args.ContainsKey("ports"))
            {
                ports = _args["ports"].Split(',').Select(x => int.Parse(x)).ToArray();
            }
            else
            {
                ports = new[] { 27017, 27018, 27019};
            }

            _args.Remove("useArbiter");
            _args.Remove("ports");

            var memberConfig = new LocalMongodConfigurationBuilder(_args)
                .ExecutablePath(Path.Combine(_binDirectory, "mongod.exe"))
                .Build();

            var replSetBuilder = new LocalReplicaSetConfigurationBuilder()
                .ReplicaSetName(replicaSetName)
                .Ports(ports, memberConfig);

            if (useArbiter)
            {
                replSetBuilder = replSetBuilder.Arbiter(ports[ports.Length - 1], memberConfig);
            }

            return replSetBuilder.Build();
        }
    }
}