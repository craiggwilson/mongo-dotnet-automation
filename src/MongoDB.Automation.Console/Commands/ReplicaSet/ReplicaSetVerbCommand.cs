using System;
using System.Collections.Generic;
using System.Configuration;
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

        [Option(LongName="dbpath")]
        public string DbPath { get; set; }

        protected ReplicaSetConfiguration GetConfiguration()
        {
            var dbPath = DbPath ?? ConfigurationManager.AppSettings["replicaset.dbpath"];
            var setName = ReplicaSetName ?? ConfigurationManager.AppSettings["replicaset.replSet"];
            var ports = Ports ?? ConfigurationManager.AppSettings["replicaset.ports"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => int.Parse(i)).ToArray();

            var memberConfig = new LocalMongodConfigurationBuilder(UnboundArguments)
                .ExecutablePath(Path.Combine(GetBinaryPath(), "mongod.exe"))
                .DbPath(dbPath)
                .Build();

            return new LocalReplicaSetConfigurationBuilder()
                .ReplicaSetName(setName)
                .Ports(ports, memberConfig)
                .Build();
        }
    }
}