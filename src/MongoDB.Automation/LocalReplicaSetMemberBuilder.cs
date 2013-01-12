using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class LocalReplicaSetMemberBuilder : LocalMongodBuilder<LocalReplicaSetMemberBuilder, ReplicaSetMemberSettings>
    {
        public LocalReplicaSetMemberBuilder(string binPath)
            : base(binPath)
        { }

        public LocalReplicaSetMemberBuilder OpLogSize(int size)
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