using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class ReplicaSetConfiguration : IControllerConfiguration
    {
        private string _replicaSetName;
        private IEnumerable<IProcessConfiguration> _members;
        private int? _arbiterPort;

        public string ReplicaSetName
        {
            get { return _replicaSetName; }
            set { _replicaSetName = value; }
        }

        public IEnumerable<IProcessConfiguration> Members
        {
            get { return _members; }
            set { _members = value; }
        }

        public int? ArbiterPort
        {
            get { return _arbiterPort; }
            set { _arbiterPort = value; }
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(_replicaSetName))
            {
                throw new ArgumentException("Cannot be null or empty.", "replicaSetName");
            }
            if (_members == null || !_members.Any())
            {
                throw new ArgumentException("Cannot be null or empty.", "processes");
            }
        }
    }
}