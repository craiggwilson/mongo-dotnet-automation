using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class ReplicaSetControllerFactory
    {
        private readonly IInstanceProcessFactory<ReplicaSetMemberSettings> _instanceProcessFactory;
        private readonly List<ReplicaSetMemberSettings> _members;

        public ReplicaSetControllerFactory(IInstanceProcessFactory<ReplicaSetMemberSettings> instanceProcessFactory)
        {
            if (instanceProcessFactory == null)
            {
                throw new ArgumentNullException("instanceProcessFactory");
            }

            _instanceProcessFactory = instanceProcessFactory;
            _members = new List<ReplicaSetMemberSettings>();
        }

        public ReplicaSetController Create()
        {
            var memberProcesses = _members.Select(x => _instanceProcessFactory.Create(x));
            return new ReplicaSetController(memberProcesses);
        }

        public ReplicaSetControllerFactory IncludeMember(ReplicaSetMemberSettings settings)
        {
            _members.Add(settings);
            return this;
        }
    }
}