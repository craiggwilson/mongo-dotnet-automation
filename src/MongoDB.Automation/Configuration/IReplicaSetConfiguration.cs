﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public interface IReplicaSetConfiguration : IControllerConfiguration
    {
        string ReplicaSetName { get; }

        IEnumerable<IProcessConfiguration> Members { get; }

        int? ArbiterPort { get; }
    }
}