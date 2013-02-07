using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public interface IReplicaSetConfiguration : IConfiguration
    {
        string ReplicaSetName { get; }

        IEnumerable<IInstanceProcessConfiguration> Members { get; }

        int? ArbiterPort { get; }
    }
}