using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation.Console.Commands.Standalone
{
    internal abstract class StandAloneVerbCommand : ClusterCommand
    {
        [Option(LongName = "dbpath")]
        public string DbPath { get; set; }

        [Option(LongName = "port")]
        public int? Port { get; set; }

        protected StandAloneConfiguration GetConfiguration()
        {
            var dbPath = DbPath ?? ConfigurationManager.AppSettings["standalone.dbpath"];
            var port = Port ?? int.Parse(ConfigurationManager.AppSettings["standalone.port"]);

            var serverConfig = new LocalMongodConfigurationBuilder(UnboundArguments)
                .ExecutablePath(Path.Combine(GetBinaryPath(), "mongod.exe"))
                .DbPath(dbPath)
                .Port(port)
                .Build();

            return new StandAloneConfiguration { Server = serverConfig };
        }
    }
}