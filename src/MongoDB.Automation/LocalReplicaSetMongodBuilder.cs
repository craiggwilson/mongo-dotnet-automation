using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class LocalReplicaSetMongodBuilder : LocalMongodBuilder<LocalReplicaSetMongodBuilder, ReplicaSetMemberSettings>
    {
        public LocalReplicaSetMongodBuilder(string binPath)
            : base(binPath)
        { }

        public LocalReplicaSetMongodBuilder OpLogSize(int size)
        {
            return Set("oplogSize", size.ToString());
        }

        protected override void ApplySettings(ReplicaSetMemberSettings settings)
        {
            base.ApplySettings(settings);
            Set("replSet", settings.ReplicaSetName);
        }
    }
}