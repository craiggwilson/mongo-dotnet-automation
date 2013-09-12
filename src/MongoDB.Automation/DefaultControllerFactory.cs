using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public class DefaultControllerFactory : IControllerFactory
    {
        private readonly IProcessFactory _processFactory;

        public DefaultControllerFactory(IProcessFactory processFactory)
        {
            if (processFactory == null)
            {
                throw new ArgumentNullException("processFactory");
            }

            _processFactory = processFactory;
        }

        public IController Create(IControllerConfiguration configuration)
        {
            if (configuration is StandAloneConfiguration)
            {
                return Create((StandAloneConfiguration)configuration);
            }
            if (configuration is ReplicaSetConfiguration)
            {
                return Create((ReplicaSetConfiguration)configuration);
            }

            throw new NotSupportedException();
        }

        private IController Create(StandAloneConfiguration configuration)
        {
            return new StandAloneController(_processFactory.Create(configuration.Server));
        }

        private IController Create(ReplicaSetConfiguration configuration)
        {
            var processes = configuration.Members.Select(x => _processFactory.Create(x));
            return new ReplicaSetController(configuration.ReplicaSetName, processes, configuration.ArbiterPort);
        }
    }
}
