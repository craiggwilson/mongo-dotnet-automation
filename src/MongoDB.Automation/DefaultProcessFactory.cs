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
            if (configuration is LocalProcessConfiguration)
            {
                return Create((LocalProcessConfiguration)configuration);
            }

            throw new NotSupportedException("Unknown configuration type.");
        }

        private IProcess Create(LocalProcessConfiguration configuration)
        {
            return new LocalProcess(configuration);
        }
    }
}