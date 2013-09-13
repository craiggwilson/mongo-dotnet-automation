using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation.Console.Commands.Standalone
{
    internal abstract class StandAloneVerbCommand : ClusterCommand
    {
        protected StandAloneConfiguration GetConfiguration()
        {
            var serverConfig = new LocalMongodConfigurationBuilder(UnboundArguments)
                .ExecutablePath(Path.Combine(GetBinaryPath(), "mongod.exe"))
                .Build();

            return new StandAloneConfiguration { Server = serverConfig };
        }
    }
}