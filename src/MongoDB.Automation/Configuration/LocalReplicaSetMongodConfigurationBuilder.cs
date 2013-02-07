using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class LocalReplicaSetMongodConfigurationBuilder : AbstractLocalMongodBuilder<LocalReplicaSetMongodConfigurationBuilder>
    {
        public LocalReplicaSetMongodConfigurationBuilder()
        { }

        public LocalReplicaSetMongodConfigurationBuilder(IEnumerable<KeyValuePair<string, string>> arguments)
            : base(arguments)
        { }

        public LocalReplicaSetMongodConfigurationBuilder(ILocalProcessConfiguration configuration)
            : base(configuration.Arguments)
        {
            BinPath(configuration.BinPath);
        }

        public LocalReplicaSetMongodConfigurationBuilder OplogSize(int sizeInMegabytes)
        {
            return Set("oplogSize", sizeInMegabytes.ToString());
        }

        public LocalReplicaSetMongodConfigurationBuilder ReplSet(string replSet)
        {
            return Set("replSet", replSet);
        }
    }
}