using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public class Automate
    {
        private readonly IInstanceProcessControllerFactory _instanceProcessControllerFactory;

        public Automate()
        {
            _instanceProcessControllerFactory = new DefaultInstanceProcessControllerFactory(new DefaultInstanceProcessFactory());
        }

        public Automate(IInstanceProcessControllerFactory instanceProcessControllerFactory)
        {
            if (instanceProcessControllerFactory == null)
            {
                throw new ArgumentNullException("instanceProcessControllerFactory");
            }

            _instanceProcessControllerFactory = instanceProcessControllerFactory;
        }

        public IInstanceProcessController From(IInstanceProcessControllerConfiguration configuration)
        {
            return _instanceProcessControllerFactory.Create(configuration);
        }

        public IInstanceProcessController BuildLocalReplicaSet(Action<LocalReplicaSetConfigurationBuilder> action)
        {
            var builder = new LocalReplicaSetConfigurationBuilder();
            action(builder);
            var configuration = builder.Build();
            return From(configuration);
        }
    }
}