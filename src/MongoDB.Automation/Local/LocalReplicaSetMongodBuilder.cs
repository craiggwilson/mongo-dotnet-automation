using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Local
{
    public class LocalReplicaSetMongodBuilder : AbstractLocalMongodBuilder<LocalReplicaSetMongodBuilder>
    {
        public LocalReplicaSetMongodBuilder OplogSize(int sizeInMegabytes)
        {
            return Set("oplogSize", sizeInMegabytes.ToString());
        }

        public LocalReplicaSetMongodBuilder ReplSet(string replSet)
        {
            return Set("replSet", replSet);
        }
    }
}