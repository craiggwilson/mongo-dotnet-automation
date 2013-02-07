using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class ReplicaSetConfiguration : IReplicaSetConfiguration
    {
        private readonly string _replicaSet;
        private readonly IEnumerable<IInstanceProcessConfiguration> _members;
        private readonly int? _arbiterPort;

        public ReplicaSetConfiguration(string replicaSet, IEnumerable<IInstanceProcessConfiguration> members)
            : this(replicaSet, members, null)
        { }

        public ReplicaSetConfiguration(string replicaSet, IEnumerable<IInstanceProcessConfiguration> members, int? arbiterPort)
        {
            _replicaSet = replicaSet;
            _members = members.ToList();
            _arbiterPort = arbiterPort;
        }

        public string ReplicaSetName
        {
            get { return _replicaSet; }
        }

        public IEnumerable<IInstanceProcessConfiguration> Members
        {
            get { return _members; }
        }

        public int? ArbiterPort
        {
            get { return _arbiterPort; }
        }
    }
}