using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class ReplicaSetConfiguration : IControllerConfiguration
    {
        private readonly string _replicaSetName;
        private readonly IEnumerable<IProcessConfiguration> _members;
        private readonly int? _arbiterPort;

        public ReplicaSetConfiguration(string replicaSetName, IEnumerable<IProcessConfiguration> members)
            : this(replicaSetName, members, null)
        { }

        public ReplicaSetConfiguration(string replicaSet, IEnumerable<IProcessConfiguration> members, int? arbiterPort)
        {
            _replicaSetName = replicaSet;
            _members = members.ToList();
            _arbiterPort = arbiterPort;
        }

        public string ReplicaSetName
        {
            get { return _replicaSetName; }
        }

        public IEnumerable<IProcessConfiguration> Members
        {
            get { return _members; }
        }

        public int? ArbiterPort
        {
            get { return _arbiterPort; }
        }
    }
}