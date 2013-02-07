using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public class DefaultInstanceProcessControllerFactory : IInstanceProcessControllerFactory
    {
        private readonly IInstanceProcessFactory _instanceProcessFactory;

        public DefaultInstanceProcessControllerFactory(IInstanceProcessFactory instanceProcessFactory)
        {
            if (instanceProcessFactory == null)
            {
                throw new ArgumentNullException("instanceProcessFactory");
            }

            _instanceProcessFactory = instanceProcessFactory;
        }

        public IInstanceProcessController Create(IInstanceProcessControllerConfiguration configuration)
        {
            if (configuration is IReplicaSetConfiguration)
            {
                return Create((IReplicaSetConfiguration)configuration);
            }

            throw new NotSupportedException();
        }

        private IInstanceProcessController Create(IReplicaSetConfiguration configuration)
        {
            var processes = configuration.Members.Select(x => _instanceProcessFactory.Create(x));
            return new ReplicaSetController(configuration.ReplicaSetName, processes, configuration.ArbiterPort);
        }
    }
}
