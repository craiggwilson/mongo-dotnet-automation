using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation.Console.Commands.ReplicaSet
{
    internal abstract class ReplicaSetVerbCommand : ClusterCommand
    {
        [Option(LongName="replSet")]
        public string ReplicaSetName { get; set; }

        [Option(LongName="ports")]
        public int[] Ports { get; set; }

        protected ReplicaSetConfiguration GetConfiguration()
        {
            var memberConfig = new LocalMongodConfigurationBuilder(UnboundArguments)
                .ExecutablePath(Path.Combine(GetBinaryPath(), "mongod.exe"))
                .Build();

            return new LocalReplicaSetConfigurationBuilder()
                .ReplicaSetName(ReplicaSetName)
                .Ports(Ports, memberConfig)
                .Build();
        }
    }
}