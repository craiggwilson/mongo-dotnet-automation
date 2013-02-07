using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public class DefaultInstanceProcessFactory : IInstanceProcessFactory
    {
        public virtual IInstanceProcess Create(IInstanceProcessConfiguration configuration)
        {
            if (configuration is ILocalInstanceProcessConfiguration)
            {
                return Create((ILocalInstanceProcessConfiguration)configuration);
            }

            throw new NotSupportedException("Unknown configuration type.");
        }

        private IInstanceProcess Create(ILocalInstanceProcessConfiguration configuration)
        {
            return new LocalInstanceProcess(configuration.BinPath, configuration.Arguments);
        }
    }
}