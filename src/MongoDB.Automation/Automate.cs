using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public class Automate
    {
        private readonly IControllerFactory _controllerFactory;

        public Automate()
        {
            _controllerFactory = new DefaultControllerFactory(new DefaultProcessFactory());
        }

        public Automate(IControllerFactory controllerFactory)
        {
            if (controllerFactory == null)
            {
                throw new ArgumentNullException("controllerFactory");
            }

            _controllerFactory = controllerFactory;
        }

        public IController From(IControllerConfiguration configuration)
        {
            return _controllerFactory.Create(configuration);
        }

        public IController BuildLocalReplicaSet(Action<LocalReplicaSetConfigurationBuilder> action)
        {
            var builder = new LocalReplicaSetConfigurationBuilder();
            action(builder);
            var configuration = builder.Build();
            return From(configuration);
        }
    }
}