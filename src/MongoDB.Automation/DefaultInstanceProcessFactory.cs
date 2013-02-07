using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public class DefaultInstanceProcessFactory : IInstanceProcessFactory
    {
        public IInstanceProcess CreateInstanceProcess(IInstanceProcessConfiguration configuration)
        {
            if (configuration is ILocalInstanceProcessConfiguration)
            {
                return new LocalInstanceProcess((ILocalInstanceProcessConfiguration)configuration);
            }

            throw new NotSupportedException("Unknown configuration type.");
        }
    }
}