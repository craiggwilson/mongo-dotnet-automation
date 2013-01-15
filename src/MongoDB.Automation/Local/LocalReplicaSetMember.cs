using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Local
{
    public class LocalReplicaSetMember
    {
        private readonly int _port;
        private readonly string _replicaSetName;

        public LocalReplicaSetMember(int port, string replicaSetName)
        {
            _port = port;
            _replicaSetName = replicaSetName;
        }

        public int Port
        {
            get { return _port; }
        }

        public string ReplicaSetName
        {
            get { return _replicaSetName; }
        }
    }
}