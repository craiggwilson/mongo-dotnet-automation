using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class ReplicaSetMemberSettings : InstanceProcessSettingsBase
    {
        private readonly bool _isArbiter;
        private readonly string _replicaSetName;

        public ReplicaSetMemberSettings(int port, string replicaSetName, bool isArbiter)
            : base(port)
        {
            if (string.IsNullOrEmpty(replicaSetName))
            {
                throw new ArgumentException("Cannot be null or empty.", "replicaSetName");
            }

            _isArbiter = isArbiter;
            _replicaSetName = replicaSetName;
        }

        public bool IsArbiter
        {
            get { return _isArbiter; }
        }

        public string ReplicaSetName
        {
            get { return _replicaSetName; }
        }
    }
}
