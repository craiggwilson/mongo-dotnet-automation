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

        public LocalReplicaSetMongodConfigurationBuilder(LocalProcessConfiguration configuration)
            : base(configuration.Arguments)
        {
            ExecutablePath(configuration.ExecutablePath);
        }

        public LocalReplicaSetMongodConfigurationBuilder OplogSize(int sizeInMegabytes)
        {
            return Set(Constants.OP_LOG_SIZE, sizeInMegabytes.ToString());
        }

        public LocalReplicaSetMongodConfigurationBuilder ReplSet(string replSet)
        {
            return Set(Constants.REPL_SET, replSet);
        }
    }
}