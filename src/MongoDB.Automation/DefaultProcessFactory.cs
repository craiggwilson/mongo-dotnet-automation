using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public class DefaultProcessFactory : IProcessFactory
    {
        public virtual IProcess Create(IProcessConfiguration configuration)
        {
            if (configuration is ILocalProcessConfiguration)
            {
                return Create((ILocalProcessConfiguration)configuration);
            }

            throw new NotSupportedException("Unknown configuration type.");
        }

        private IProcess Create(ILocalProcessConfiguration configuration)
        {
            return new LocalProcess(configuration.ExecutablePath, configuration.Arguments);
        }
    }
}