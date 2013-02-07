using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public interface IReplicaSetConfiguration : IInstanceProcessControllerConfiguration
    {
        string ReplicaSetName { get; }

        IEnumerable<IInstanceProcessConfiguration> Members { get; }

        int? ArbiterPort { get; }
    }
}